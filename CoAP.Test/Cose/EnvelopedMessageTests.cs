using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeterO.Cbor;
using Com.AugustCellars.COSE;

namespace COSETests
{
    /// <summary>
    /// Summary description for EnvelopedMessageTests
    /// </summary>
    [TestClass]
    public class EnvelopedMessageTests
    {
        byte[] _rgbKey128 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        byte[] _rgbKey256 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
        String _strContent = "This is some content";
        byte[] _rgbIv128 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        byte[] _rgbIv96 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        CBORObject _cnKey128;
        Key _key128;

        [TestInitialize()]
        public void Setup()
        {
            _cnKey128 = CBORObject.NewMap();
            _cnKey128.Add(CoseKeyKeys.KeyType, GeneralValues.KeyType_Octet);
            _cnKey128.Add(CoseKeyParameterKeys.Octet_k, CBORObject.FromObject(_rgbKey128));
            _key128 = new Key(_cnKey128);
        }

#if false
        [TestMethod()]
        public void EnvelopedMessageTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DecodeFromCBORObjectTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void EncodeTest()
        {
            Assert.Fail();
        }
#endif

        [TestMethod()]
        public void DecodeWrongBasis()
        {
            CBORObject obj = CBORObject.NewMap();

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => (Message.DecodeFromBytes(rgb, Tags.Encrypt)));
        }

        [TestMethod()]
        public void DecodeWrongCount()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => (Message.DecodeFromBytes(rgb, Tags.Encrypt)));
        }

        [TestMethod()]
        public void DecodeBadProtected()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => (Message.DecodeFromBytes(rgb, Tags.Encrypt)));
        }

        [TestMethod()]
        public void DecodeBadProtected2()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.FromObject(CBORObject.False.EncodeToBytes()));
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => (Message.DecodeFromBytes(rgb, Tags.Encrypt)));
        }

        [TestMethod()]
        public void DecodeBadUnprotected()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.FromObject(CBORObject.NewMap().EncodeToBytes()));
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => (Message.DecodeFromBytes(rgb, Tags.Encrypt)));
        }

        [TestMethod()]
        public void DecodeBadContent()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.FromObject(new byte[0]));
            obj.Add(CBORObject.NewMap());
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => (Message.DecodeFromBytes(rgb, Tags.Encrypt)));
        }

        [TestMethod()]
        public void DecodeBadRecipients()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.FromObject(new byte[0]));
            obj.Add(CBORObject.NewMap());
            obj.Add(CBORObject.Null);
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => (Message.DecodeFromBytes(rgb, Tags.Encrypt)));
        }

        [TestMethod()]
        public void NoAlgorithm()
        {
            EncryptMessage msg = new EncryptMessage();
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            Assert.Throws<CoseException>(() => msg.Encrypt());
        }

        [TestMethod()]
        public void UnknownAlgorithm()
        {
            EncryptMessage msg = new EncryptMessage();
            msg.AddAttribute(HeaderKeys.Algorithm, CBORObject.FromObject("Unknown"), Attributes.PROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            Assert.Throws<CoseException>(() => msg.Encrypt());
        }

        [TestMethod()]
        public void UnsupportedAlgorithm()
        {
            EncryptMessage msg = new EncryptMessage();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.HMAC_SHA_256, Attributes.PROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            Assert.Throws<CoseException>(() => msg.Encrypt());
        }

        [TestMethod()]
        public void NullKey()
        {
            EncryptMessage msg = new EncryptMessage();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.SetContent(_strContent);
            Assert.Throws<CoseException>(() => msg.Encrypt());
        }

        [TestMethod()] 
        public void NoContent()
        {
            EncryptMessage msg = new EncryptMessage();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            Assert.Throws<CoseException>(() => msg.Encrypt());
        }

        [TestMethod()]
        public void BadIv()
        {
            EncryptMessage msg = new EncryptMessage();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject("IV"), Attributes.UNPROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            Assert.Throws<CoseException>(() => msg.Encrypt());
        }

        [TestMethod()]
        public void IncorrectIv()
        {
            EncryptMessage msg = new EncryptMessage();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(_rgbIv128), Attributes.UNPROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            Assert.Throws<CoseException>(() => msg.Encrypt());
        }

        [TestMethod()]
        public void EncryptNoTag()
        {
            EncryptMessage msg = new EncryptMessage(false, true);

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(_rgbIv96), Attributes.UNPROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            msg.Encrypt();
            CBORObject cn = msg.EncodeToCBORObject();


            Assert.IsFalse(cn.IsTagged);
        }

        [TestMethod()]
        public void EncryptNoEmitContent()
        {
            EncryptMessage msg = new EncryptMessage(true, false);

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(_rgbIv96), Attributes.UNPROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            msg.Encrypt();
            CBORObject cn = msg.EncodeToCBORObject();


            Assert.IsTrue(cn[2].IsNull);
        }

        [TestMethod()]
        public void NoContentForDecrypt()
        {
            EncryptMessage msg = new EncryptMessage(true, false);

            //        thrown.expect(CoseException.class);
            //        thrown.expectMessage("No Enveloped Content Specified");

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(_rgbIv96), Attributes.UNPROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            msg.Encrypt();

            byte[] rgb = msg.EncodeToBytes();

            msg = (EncryptMessage) Message.DecodeFromBytes(rgb);
            r = msg.RecipientList[0];
            r.SetKey(_key128);
            Assert.Throws<CoseException>(() => msg.Decrypt(r));

        }

        [TestMethod()]
        public void NullKeyForDecrypt()
        {
            EncryptMessage msg = new EncryptMessage(true, true);

            //        thrown.expect(CoseException.class);
            //        thrown.expectMessage("No Enveloped Content Specified");

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(_rgbIv96), Attributes.UNPROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            msg.Encrypt();

            byte[] rgb = msg.EncodeToBytes();

            msg = (EncryptMessage) Message.DecodeFromBytes(rgb);
            Assert.Throws<CoseException>(() => msg.Decrypt(null));

        }

        [TestMethod()]
        public void RoundTripDetached()
        {
            EncryptMessage msg = new EncryptMessage(true, false);

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(_rgbIv96), Attributes.UNPROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            msg.Encrypt();

            byte[] content = msg.GetEncryptedContent();

            byte[] rgb = msg.EncodeToBytes();

            msg = (EncryptMessage) Message.DecodeFromBytes(rgb);
            msg.SetEncryptedContent(content);
            r = msg.RecipientList[0];
            r.SetKey(_key128);
            msg.Decrypt(r);

        }

        [TestMethod()]
        public void RoundTrip()
        {
            EncryptMessage msg = new EncryptMessage();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(_rgbIv96), Attributes.UNPROTECTED);
            msg.SetContent(_strContent);
            Recipient r = new Recipient(_key128, AlgorithmValues.Direct);
            msg.AddRecipient(r);
            msg.Encrypt();
            byte[] rgbMsg = msg.EncodeToBytes();

            msg = (EncryptMessage) Message.DecodeFromBytes(rgbMsg);
            r = msg.RecipientList[0];
            r.SetKey(_key128);
            msg.Decrypt(r);

            Assert.AreEqual<string>(msg.GetContentAsString(), _strContent);
        }
    }
}
