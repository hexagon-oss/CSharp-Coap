using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto.Tls;

using Com.AugustCellars.COSE;
#if SUPPORT_TLS_CWT
using Com.AugustCellars.WebToken.CWT;
#endif
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.X509;
using PeterO.Cbor;

namespace Com.AugustCellars.CoAP.DTLS
{
    public class DtlsServer : DefaultTlsServer
    {
        private readonly TlsKeyPairSet _serverKeys;
        private KeySet _userKeys;

        public EventHandler<TlsEvent> TlsEventHandler;

        public KeySet CwtTrustKeySet { get; set; }

        public DtlsServer(TlsKeyPairSet serverKeys, KeySet userKeys)
			: base(new BcTlsCrypto(new SecureRandom()))
        {
            _serverKeys = serverKeys;
            _userKeys = userKeys;
            mPskIdentityManager = new MyIdentityManager(userKeys);
            mPskIdentityManager.TlsEventHandler += OnTlsEvent;
        }

        protected ProtocolVersion MinimumVersion => ProtocolVersion.DTLSv10;
        protected ProtocolVersion MaximumVersion => ProtocolVersion.DTLSv12;

        public OneKey AuthenticationKey => mPskIdentityManager.AuthenticationKey;
        public Certificate AuthenticationCertificate { get; private set; }

        // Chain all of our events to the next level up.

        private void OnTlsEvent(Object o, TlsEvent e)
        {
            EventHandler<TlsEvent> handler = TlsEventHandler;
            if (handler != null) {
                handler(o, e);
            }
        }

        public override int[] GetCipherSuites()
        {
            int[] i = new int[] {
                CipherSuite.TLS_PSK_WITH_AES_128_CCM_8,
                CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8,
                CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CCM_8
            };

            //  Give the outside code a chance to change this.

            TlsEvent e = new TlsEvent(TlsEvent.EventCode.GetCipherSuites) {
                IntValues = i
            };

            EventHandler<TlsEvent> handler = TlsEventHandler;
            if (handler != null) {
                handler(this, e);
            }

            return e.IntValues;
        }

        public override void NotifyFallback(bool isFallback)
        {
            // M00BUG - Do we care?
            return;
        }

        public override void NotifySecureRenegotiation(bool secureRenegotiation)
        {
            // M00BUG - do we care ?
            return;
        }

        public override TlsCredentials GetCredentials()
        {
            int keyExchangeAlgorithm = TlsUtilities.GetKeyExchangeAlgorithm(m_selectedCipherSuite);

            switch (keyExchangeAlgorithm) {
            case KeyExchangeAlgorithm.DHE_PSK:
            case KeyExchangeAlgorithm.ECDHE_PSK:
            case KeyExchangeAlgorithm.PSK:
                return null;

            case KeyExchangeAlgorithm.RSA_PSK:
	            return GetRsaEncryptionCredentials();

            default:
                /* Note: internal error here; selected a key exchange we don't implement! */
                throw new TlsFatalAlert(AlertDescription.internal_error);
            }
        }

#if SUPPORT_TLS_CWT
        public override AbstractCertificate ParseCertificate(short certificateType, Stream io)
        {
            switch (certificateType)
            {
            case CertificateType.CwtPublicKey:
                try
                {
                    CwtPublicKey cwtPub = CwtPublicKey.Parse(io);

                    Cwt cwtServer = Cwt.Decode(cwtPub.EncodedCwt(), CwtTrustKeySet, CwtTrustKeySet);

                    AsymmetricKeyParameter pubKey = cwtServer.Cnf.CoseKey.AsPublicKey();

                    SubjectPublicKeyInfo spi = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pubKey);
                    cwtPub.SetSubjectPublicKeyInfo(spi);

                    return cwtPub;
                }
                catch
                {
                    return null;
                }

            default:
                return null;
            }
        }
#endif
        protected override TlsCredentialedSigner GetECDsaSignerCredentials()
        {
            throw new TlsFatalAlert(AlertDescription.internal_error);
        }

        public override CertificateRequest GetCertificateRequest()
        {
            short[] certificateTypes = new short[]{ ClientCertificateType.rsa_sign,
                ClientCertificateType.ecdsa_sign };

            IList<SignatureAndHashAlgorithm> serverSigAlgs = null;
            if (TlsUtilities.IsSignatureAlgorithmsExtensionAllowed(GetServerVersion())) {
                serverSigAlgs = TlsUtilities.GetDefaultSupportedSignatureAlgorithms(m_context);
            }

            return new CertificateRequest(certificateTypes, serverSigAlgs, null);
        }

#if SUPPORT_RPK
        public override void NotifyClientCertificate(AbstractCertificate clientCertificate)
        {
            if (clientCertificate is RawPublicKey) {
                mPskIdentityManager.GetRpkKey((RawPublicKey) clientCertificate);
            }
#if SUPPORT_TLS_CWT
            else if (clientCertificate is CwtPublicKey) {
                mPskIdentityManager.CwtTrustRoots = CwtTrustKeySet;
                mPskIdentityManager.GetCwtKey((CwtPublicKey) clientCertificate);
            }
#endif
            else if (clientCertificate is Certificate) {
                TlsEvent e = new TlsEvent(TlsEvent.EventCode.ClientCertificate) {
                    Certificate = clientCertificate,
                    CertificateType = CertificateType.X509
                };

                EventHandler<TlsEvent> handler = TlsEventHandler;
                if (handler != null) {
                    handler(this, e);
                }

                if (!e.Processed) {
                    throw new TlsFatalAlert(AlertDescription.certificate_unknown);
                }

                AuthenticationCertificate = (Certificate) clientCertificate;
            }
            else {
                throw new TlsFatalAlert(AlertDescription.certificate_unknown);
            }
        }
#else
        public override void NotifyClientCertificate(Certificate clientCertificate)
        {
                TlsEvent e = new TlsEvent(TlsEvent.EventCode.ClientCertificate) {
                    Certificate = clientCertificate
                };

                EventHandler<TlsEvent> handler = TlsEventHandler;
                if (handler != null) {
                    handler(this, e);
                }

                if (!e.Processed) {
                    throw new TlsFatalAlert(AlertDescription.certificate_unknown);
                }

                AuthenticationCertificate = (Certificate) clientCertificate;
            
                AuthenticationCertificate = (Certificate) clientCertificate;
            
        }
#endif

        private MyIdentityManager mPskIdentityManager;

        internal class MyIdentityManager
            : TlsPskIdentityManager
        {
            private KeySet _userKeys;
            public EventHandler<TlsEvent> TlsEventHandler;

            internal MyIdentityManager(KeySet keys)
            {
                _userKeys = keys;
#if SUPPORT_TLS_CWT
                CwtAuthenticationKey = null;
#endif
            }

            public OneKey AuthenticationKey { get; private set; }

#if SUPPORT_TLS_CWT
            public KeySet CwtTrustRoots { get; set; }
            public Cwt CwtAuthenticationKey { get; }
#endif

            public virtual byte[] GetHint()
            {
                return Encoding.UTF8.GetBytes("hint");
            }

            public virtual byte[] GetPsk(byte[] identity)
            {
                foreach (OneKey key in _userKeys) {
                    if (!key.HasKeyType((int) GeneralValuesInt.KeyType_Octet)) continue;

                    if (identity == null) {
                        if (key.HasKid(null)) {
                            AuthenticationKey = key;
                            return (byte[]) key[CoseKeyParameterKeys.Octet_k].GetByteString().Clone();
                        }
                    }
                    else {
                        if (key.HasKid(identity)) {
                            AuthenticationKey = key;
                            return (byte[]) key[CoseKeyParameterKeys.Octet_k].GetByteString().Clone();
                        }
                    }
                }


                TlsEvent e = new TlsEvent(TlsEvent.EventCode.UnknownPskName) {
                    PskName = identity
                };
                EventHandler<TlsEvent> handler = TlsEventHandler;
                if (handler != null) {
                    handler(this, e);
                }

                if (e.KeyValue != null) {
                    if (e.KeyValue.HasKeyType((int) GeneralValuesInt.KeyType_Octet)) {
                        AuthenticationKey = e.KeyValue;
                        return (byte[]) e.KeyValue[CoseKeyParameterKeys.Octet_k].GetByteString().Clone();
                    }
                }

                return null;
            }

            public void GetCertKey(Certificate certificate)
            {

            }

#if SUPPORT_RPK
            public void GetRpkKey(RawPublicKey rpk)
            {
                AsymmetricKeyParameter key;

                try {
                    key = PublicKeyFactory.CreateKey(rpk.SubjectPublicKeyInfo());
                }
                catch (Exception e) {
                    throw new TlsFatalAlert(AlertDescription.unsupported_certificate, e);
                }

                if (key is ECPublicKeyParameters) {
                    ECPublicKeyParameters ecKey = (ECPublicKeyParameters) key;

                    string s = ecKey.AlgorithmName;
                    OneKey newKey = new OneKey();
                    newKey.Add(CoseKeyKeys.KeyType, GeneralValues.KeyType_EC);
                    if (ecKey.Parameters.Curve.Equals(NistNamedCurves.GetByName("P-256").Curve)) {
                        newKey.Add(CoseKeyParameterKeys.EC_Curve, GeneralValues.P256);
                    }

                    newKey.Add(CoseKeyParameterKeys.EC_X, CBORObject.FromObject(ecKey.Q.Normalize().XCoord.ToBigInteger().ToByteArrayUnsigned()));
                    newKey.Add(CoseKeyParameterKeys.EC_Y,  CBORObject.FromObject(ecKey.Q.Normalize().YCoord.ToBigInteger().ToByteArrayUnsigned()));

                    foreach (OneKey k in _userKeys) {
                        if (k.Compare(newKey)) {
                            AuthenticationKey = k;
                            return;
                        }
                    }
                }
                else {
                    // throw new TlsFatalAlert(AlertDescription.certificate_unknown);
                }

                TlsEvent ev = new TlsEvent(TlsEvent.EventCode.ClientCertificate) {
                    Certificate = rpk
                };

                EventHandler<TlsEvent> handler = TlsEventHandler;
                if (handler != null) {
                    handler(this, ev);
                }

                if (!ev.Processed) {
                    throw new TlsFatalAlert(AlertDescription.certificate_unknown);
                }
            }
#endif

#if SUPPORT_TLS_CWT
            public void GetCwtKey(CwtPublicKey rpk)
            {
                Cwt cwt;

                try {
                    cwt = Cwt.Decode(rpk.EncodedCwt(), CwtTrustRoots, CwtTrustRoots);

                    AuthenticationKey = cwt.Cnf.CoseKey;
                }
                catch (Exception e)
                {
                    TlsEvent ev = new TlsEvent(TlsEvent.EventCode.ClientCertificate)
                    {
                        Certificate = rpk
                    };

                    EventHandler<TlsEvent> handler = TlsEventHandler;
                    if (handler != null)
                    {
                        handler(this, ev);
                    }

                    if (!ev.Processed)
                    {
                        throw new TlsFatalAlert(AlertDescription.certificate_unknown);
                    }

                    AuthenticationKey = ev.KeyValue;
                }
            }
#endif

        }
    }
}
