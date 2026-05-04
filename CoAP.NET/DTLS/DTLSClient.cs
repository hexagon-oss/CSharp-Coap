using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com.AugustCellars.COSE;
#if SUPPORT_TLS_CWT
using Com.AugustCellars.WebToken.CWT;
using Com.AugustCellars.WebToken;
#endif
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using PeterO.Cbor;


namespace Com.AugustCellars.CoAP.DTLS
{

    public class DtlsClient : DefaultTlsClient
    {
        private readonly TlsPskIdentity _mPskIdentity;
        private TlsSession _mSession;
        public EventHandler<TlsEvent> TlsEventHandler;
        private readonly TlsKeyPair _tlsKeyPair;

        public DtlsClient(TlsSession session, TlsPskIdentity pskIdentity)
			: base(new BcTlsCrypto(new SecureRandom()))
        {
            _mSession = session;
            _mPskIdentity = pskIdentity;
        }

        public DtlsClient(TlsSession session, TlsKeyPair userKey)
			: base(new BcTlsCrypto(new SecureRandom()))
        {
            _mSession = session;
            _tlsKeyPair = userKey ?? throw new ArgumentNullException(nameof(userKey));
        }

#if SUPPORT_TLS_CWT
        public KeySet CwtTrustKeySet { get; set; }
        public DtlsClient(TlsSession session, TlsKeyPair tlsKey, KeySet cwtTrustKeys)
        {
            _mSession = session;
            _tlsKeyPair = tlsKey ?? throw new ArgumentNullException(nameof(tlsKey));
            CwtTrustKeySet = cwtTrustKeys;
        }
#endif

        public ProtocolVersion MinimumVersion => ProtocolVersion.DTLSv10;

        public ProtocolVersion ClientVersion => ProtocolVersion.DTLSv12;

        public override int[] GetCipherSuites()
        {
            int[] i;

            if (_tlsKeyPair != null) {
                if (_tlsKeyPair.X509Certificate != null) {
                    i = new int[] {
                    CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8,
                    CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CCM_8
                };
            }
#if SUPPORT_RPK
                else if (_tlsKeyPair.PublicKey != null) {
                    i = new int[] {
                        CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8,
                        CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CCM_8
                    };
                }
#endif
#if SUPPORT_TLS_CWT
                else if (_tlsKeyPair.CertType == CertificateType.CwtPublicKey) {
                i = new int[] {
                    CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8,
                    CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CCM_8
                };
            }
#endif
                else {
                    //  We should never get here
                    i = new int[0];
                }
            }
            else {
                    //  We should never get here
                    i = new int[] {
                        CipherSuite.TLS_PSK_WITH_AES_128_CCM_8
                    };
            }
 
            TlsEvent e = new TlsEvent(TlsEvent.EventCode.GetCipherSuites) {
                IntValues = i
            };

            EventHandler<TlsEvent> handler = TlsEventHandler;
            if (handler != null) {
                handler(this, e);
            }

            return e.IntValues;
        }

#if SUPPORT_TLS_CWT
        public override AbstractCertificate ParseServerCertificate(short certificateType, Stream io)
        {
            switch (certificateType) {
            case CertificateType.CwtPublicKey:
                try {
                    CwtPublicKey cwtPub = CwtPublicKey.Parse(io);

                    Cwt cwtServer = Cwt.Decode(cwtPub.EncodedCwt(), CwtTrustKeySet, CwtTrustKeySet);

                    AsymmetricKeyParameter pubKey = cwtServer.Cnf.CoseKey.AsPublicKey();

                    SubjectPublicKeyInfo spi = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pubKey);
                    cwtPub.SetSubjectPublicKeyInfo(spi);

                    return cwtPub;
                }
                catch {
                    return null;
                }

            default:
                return null;
            }
        }
#endif

        /// <summary>
        /// Decide which type of client and server certificates are going to be supported.
        /// By default, we assume that only those certificate types which match the clients
        /// certificate are going to be supported for the server.
        /// </summary>
        /// <returns></returns>
        public override IDictionary<int, byte[]> GetClientExtensions()
        {
            IDictionary<int, byte[]> clientExtensions = TlsExtensionsUtilities.EnsureExtensionsInitialised(base.GetClientExtensions());


            // TlsExtensionsUtilities.AddEncryptThenMacExtension(clientExtensions);
            // TlsExtensionsUtilities.AddExtendedMasterSecretExtension(clientExtensions);
            {
                /*
                 * NOTE: If you are copying test code, do not blindly set these extensions in your own client.
                 */
                //   TlsExtensionsUtilities.AddMaxFragmentLengthExtension(clientExtensions, MaxFragmentLength.pow2_9);
                //    TlsExtensionsUtilities.AddPaddingExtension(clientExtensions, mContext.SecureRandom.Next(16));
                //    TlsExtensionsUtilities.AddTruncatedHMacExtension(clientExtensions);

#if SUPPORT_RPK
                if (_tlsKeyPair != null && _tlsKeyPair.CertType == CertificateType.RawPublicKey) {
                    TlsExtensionsUtilities.AddClientCertificateTypeExtensionClient(clientExtensions, new byte[] {2});
                    TlsExtensionsUtilities.AddServerCertificateTypeExtensionClient(clientExtensions, new byte[] {2});
                }
#endif

#if SUPPORT_TLS_CWT
                if (_tlsKeyPair != null && _tlsKeyPair.CertType == CertificateType.CwtPublicKey) {
                    TlsExtensionsUtilities.AddClientCertificateTypeExtensionClient(clientExtensions, new byte[] {254});
                    TlsExtensionsUtilities.AddServerCertificateTypeExtensionClient(clientExtensions, new byte[] {254});
                }
#endif
            }

            TlsEvent e = new TlsEvent(TlsEvent.EventCode.GetExtensions) {
                Dictionary = clientExtensions
            };


            EventHandler<TlsEvent> handler = TlsEventHandler;
            if (handler != null) {
                handler(this, e);
            }

            return e.Dictionary;
        }

        public override TlsAuthentication GetAuthentication()
        {
#if SUPPORT_RPK
            if (_tlsKeyPair != null && _tlsKeyPair.CertType == CertificateType.RawPublicKey) {
                MyTlsAuthentication auth = new MyTlsAuthentication(mContext, _tlsKeyPair);
                auth.TlsEventHandler += MyTlsEventHandler;
                return auth;
            }
#endif
#if SUPPORT_TLS_CWT
            if (_tlsKeyPair != null && _tlsKeyPair.CertType == CertificateType.CwtPublicKey) {
                MyTlsAuthentication auth = new MyTlsAuthentication(mContext, _tlsKeyPair, CwtTrustKeySet);
                auth.TlsEventHandler += MyTlsEventHandler;
                return auth;
            }
#endif
            if (_tlsKeyPair != null && _tlsKeyPair.CertType == CertificateType.X509) {
                MyTlsAuthentication auth = new MyTlsAuthentication(m_context, _tlsKeyPair);
                auth.TlsEventHandler += MyTlsEventHandler;
                return auth;
            }

            throw new CoAPException("ICE");
        }

        private void MyTlsEventHandler(object sender, TlsEvent tlsEvent)
        {
            EventHandler<TlsEvent> handler = TlsEventHandler;
            if (handler != null) {
                handler(sender, tlsEvent);
            }
        }

        /// <summary>
        /// We don't care if we cannot do secure renegotiation at this time.
        /// This needs to be reviewed in the future M00TODO
        /// </summary>
        /// <param name="secureRenegotiation"></param>
        public override void NotifySecureRenegotiation(bool secureRenegotiation)
        {
            //  M00TODO - should we care?
        }


        internal class MyTlsAuthentication
	        : TlsAuthentication
        {
	        private readonly TlsContext _mContext;
	        public EventHandler<TlsEvent> TlsEventHandler;
#if SUPPORT_RPK || SUPPORT_TLS_CWT
            private KeySet _serverKeys;
#endif
	        private TlsKeyPair TlsKey { get; set; }
#if SUPPORT_TLS_CWT
            public KeySet CwtTrustKeySet { get; set; }
#endif

	        internal MyTlsAuthentication(TlsContext context, TlsKeyPair rawPublicKey)
	        {
		        this._mContext = context;
		        TlsKey = rawPublicKey;
	        }

#if SUPPORT_TLS_CWT
            internal MyTlsAuthentication(TlsContext context, TlsKeyPair cwt, KeySet trustKeys)
            {
                this._mContext = context;
                TlsKey = cwt;
                CwtTrustKeySet = trustKeys;
            }
#endif

	        public OneKey AuthenticationKey { get; private set; }

	        public virtual TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
	        {
		        if (certificateRequest.CertificateTypes == null ||
		            !Arrays.Contains(certificateRequest.CertificateTypes, ClientCertificateType.ecdsa_sign))
		        {
			        return null;
		        }

		        throw new NotImplementedException();

		        ////if (TlsKey != null)
		        ////{
		        ////	if (TlsKey.CertType == CertificateType.X509)
		        ////	{

		        ////		return new DefaultTlsSignerCredentials(_mContext, new Certificate(TlsKey.X509Certificate), TlsKey.PrivateKey.AsPrivateKey(),
		        ////			new SignatureAndHashAlgorithm(HashAlgorithm.sha256, SignatureAlgorithm.ecdsa));
		        ////	}
		        ////}

		        ////// If we did not fine appropriate signer credentials - ask for help

		        ////TlsEvent e = new TlsEvent(TlsEvent.EventCode.SignCredentials)
		        ////{
		        ////	CipherSuite = KeyExchangeAlgorithm.ECDHE_ECDSA
		        ////};

		        ////EventHandler<TlsEvent> handler = TlsEventHandler;
		        ////if (handler != null)
		        ////{
		        ////	handler(this, e);
		        ////}

		        ////if (e.SignerCredentials != null) return e.SignerCredentials;
		        ////throw new TlsFatalAlert(AlertDescription.internal_error);
	        }



	        public virtual void NotifyServerCertificate(TlsServerCertificate serverCertificate)
	        {
		        TlsEvent e = new TlsEvent(TlsEvent.EventCode.ServerCertificate)
		        {
			        Certificate = serverCertificate.Certificate,
		        };

		        EventHandler<TlsEvent> handler = TlsEventHandler;
		        if (handler != null)
		        {
			        handler(this, e);
		        }

		        if (!e.Processed)
		        {
			        throw new TlsFatalAlert(AlertDescription.certificate_unknown);
		        }

	        }

	        private static BigInteger ConvertBigNum(CBORObject cbor)
	        {
		        byte[] rgb = cbor.GetByteString();
		        byte[] rgb2 = new byte[rgb.Length + 2];
		        rgb2[0] = 0;
		        rgb2[1] = 0;
		        for (int i = 0; i < rgb.Length; i++)
		        {
			        rgb2[i + 2] = rgb[i];
		        }

		        return new BigInteger(rgb2);
	        }
        }
    }
}

