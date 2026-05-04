using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.AugustCellars.COSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeterO.Cbor;

namespace COSE.Tests
{
    [TestClass()]
    public class EncryptMessageTests
    {
        byte[] rgbKey128 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        byte[] rgbKey256 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
        String strContent = "This is some content";
        byte[] rgbIV128 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        byte[] rgbIV96 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

#if false
        [TestMethod()]
        public void EncryptMessageTest()
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
        public void decodeWrongBasis()
        {
            CBORObject obj = CBORObject.NewMap();

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => Message.DecodeFromBytes(rgb, Tags.Encrypt0));
        }

        [TestMethod()]
        public void decodeWrongCount()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => Message.DecodeFromBytes(rgb, Tags.Encrypt0));
        }

        [TestMethod()]
        public void decodeBadProtected()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => Message.DecodeFromBytes(rgb, Tags.Encrypt0));
        }

        [TestMethod()]
        public void decodeBadProtected2()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.FromObject(CBORObject.False.EncodeToBytes()));
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => Message.DecodeFromBytes(rgb, Tags.Encrypt0));
        }

        [TestMethod()]
        public void decodeBadUnprotected()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.FromObject(CBORObject.NewMap().EncodeToBytes()));
            obj.Add(CBORObject.False);
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => Message.DecodeFromBytes(rgb, Tags.Encrypt0));
        }

        [TestMethod()]
        public void decodeBadContent()
        {
            CBORObject obj = CBORObject.NewArray();
            obj.Add(CBORObject.FromObject(new byte[0]));
            obj.Add(CBORObject.NewMap());
            obj.Add(CBORObject.False);

            byte[] rgb = obj.EncodeToBytes();
            Assert.Throws<CoseException>(() => Message.DecodeFromBytes(rgb, Tags.Encrypt0));
        }

        [TestMethod()]
        public void noAlgorithm()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.SetContent(strContent);
            Assert.Throws<CoseException>(() => msg.Encrypt(rgbKey128));
        }

        [TestMethod()]
        public void unknownAlgorithm()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.AddAttribute(HeaderKeys.Algorithm, CBORObject.FromObject("Unknown"), Attributes.PROTECTED);
            msg.SetContent(strContent);
            Assert.Throws<CoseException>(() => msg.Encrypt(rgbKey128));
        }

        [TestMethod()]
        public void unsupportedAlgorithm()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.HMAC_SHA_256, Attributes.PROTECTED);
            msg.SetContent(strContent);
            Assert.Throws<CoseException>(() => msg.Encrypt(rgbKey128));
        }

        [TestMethod()]
        public void incorrectKeySize()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.SetContent(strContent);
            Assert.Throws<CoseException>(() => msg.Encrypt(rgbKey256));
        }

        [TestMethod()]
        public void nullKey()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.SetContent(strContent);
            Assert.Throws<CoseException>(() => msg.Encrypt(null));
        }

        [TestMethod()]
        public void noContent()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            Assert.Throws<CoseException>(() => msg.Encrypt(rgbKey128));
        }

        [TestMethod()]
        public void badIV()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject("IV"), Attributes.UNPROTECTED);
            msg.SetContent(strContent);
            Assert.Throws<CoseException>(() => msg.Encrypt(rgbKey128));
        }

        [TestMethod()]
        public void incorrectIV()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(rgbIV128), Attributes.UNPROTECTED);
            msg.SetContent(strContent);
            Assert.Throws<CoseException>(() => msg.Encrypt(rgbKey128));
        }

        [TestMethod()]
        public void encryptNoTag() {
            Encrypt0Message msg = new Encrypt0Message(false, true);

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(rgbIV96), Attributes.UNPROTECTED);
            msg.SetContent(strContent);
            msg.Encrypt(rgbKey128);
            CBORObject cn = msg.EncodeToCBORObject();


            Assert.IsFalse(cn.IsTagged);
        }

        [TestMethod()]
        public void encryptNoEmitContent()
        {
            Encrypt0Message msg = new Encrypt0Message(true, false);

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(rgbIV96), Attributes.UNPROTECTED);
            msg.SetContent(strContent);
            msg.Encrypt(rgbKey128);
            CBORObject cn = msg.EncodeToCBORObject();


            Assert.IsTrue(cn[2].IsNull);
        }

        [TestMethod()]
        public void noContentForDecrypt()
        {
            Encrypt0Message msg = new Encrypt0Message(true, false);

            //        thrown.expect(CoseException.class);
            //        thrown.expectMessage("No Encrypted Content Specified");

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(rgbIV96), Attributes.UNPROTECTED);
            msg.SetContent(strContent);
            msg.Encrypt(rgbKey128);

            byte[] rgb = msg.EncodeToBytes();

            msg = (Encrypt0Message) Message.DecodeFromBytes(rgb);
            Assert.Throws<CoseException>(() => msg.Decrypt(rgbKey128));

        }

        [TestMethod()]
        public void nullKeyForDecrypt()
        {
            Encrypt0Message msg = new Encrypt0Message(true, true);

            //        thrown.expect(CoseException.class);
            //        thrown.expectMessage("No Encrypted Content Specified");

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(rgbIV96), Attributes.UNPROTECTED);
            msg.SetContent(strContent);
            msg.Encrypt(rgbKey128);

            byte[] rgb = msg.EncodeToBytes();

            msg = (Encrypt0Message) Message.DecodeFromBytes(rgb);
            Assert.Throws<CoseException>(() => msg.Decrypt(null));

        }

        [TestMethod()]
        public void roundTripDetached()
        {
            Encrypt0Message msg = new Encrypt0Message(true, false);

            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(rgbIV96), Attributes.UNPROTECTED);
            msg.SetContent(strContent);
            msg.Encrypt(rgbKey128);

            byte[] content = msg.GetEncryptedContent();

            byte[] rgb = msg.EncodeToBytes();

            msg = (Encrypt0Message) Message.DecodeFromBytes(rgb);
            msg.SetEncryptedContent(content);
            msg.Decrypt(rgbKey128);

        }    

        [TestMethod()]
        public void roundTrip()
        {
            Encrypt0Message msg = new Encrypt0Message();
            msg.AddAttribute(HeaderKeys.Algorithm, AlgorithmValues.AES_GCM_128, Attributes.PROTECTED);
            msg.AddAttribute(HeaderKeys.IV, CBORObject.FromObject(rgbIV96), Attributes.UNPROTECTED);
            msg.SetContent(strContent);
            msg.Encrypt(rgbKey128);
            byte[] rgbMsg = msg.EncodeToBytes();

            msg = (Encrypt0Message) Message.DecodeFromBytes(rgbMsg);
            msg.Decrypt(rgbKey128);

            Assert.AreEqual<string>(msg.GetContentAsString(), strContent);
        }
    }
}
