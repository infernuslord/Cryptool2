using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;
using Cryptool.PluginBase;

namespace PKCS1.Library
{
    class KuehnSignature : Signature, IGuiLogMsg
    {
        public KuehnSignature()
        {
            this.registerHandOff();
        }

        public override void GenerateSignature()
        {
            this.m_KeyLength = RSAKeyManager.getInstance().RsaKeySize; // Länge des RSA Modulus
            string message = Datablock.getInstance().Message; // Die Nachricht für welche die Signatur erstellt werden soll
            HashFunctionIdent hashFuncIdent = Datablock.getInstance().HashFunctionIdent; // Die vom User gewählte Hashfunktion
            byte[] hashIdent = Hex.Decode(Encoding.ASCII.GetBytes(hashFuncIdent.DERIdent)); // ASN.1 codierter Ident-string

            // Datenblock wird konstruiert
            byte[] A = new byte[46];
            A[0] = 0x00;
            A[1] = 0x01;
            for (int i = 2; i < 10; i++)
            {
                A[i] = 0xFF;
            }
            A[10] = 0x00;         
            Array.Copy(hashIdent, 0, A, 11, hashIdent.Length);          
            // Datenblock noch ohne Hashwert, wird in while Schleife unten hinzugefügt

            // byte array der kompletten Signatur, wird zuerst mit 'FF' gefüllt und dann nachher Datenblock an den Anfang kopiert
            byte[] S = new byte[128]; // 1024 bit
            for (int i = A.Length; i < S.Length; i++)
            {
                S[i] = 0xFF;
            }

            BigInteger finalSignature = null;
            byte[] bMessage = Encoding.ASCII.GetBytes(message);
            int iMsgLength = bMessage.Length;

            bool isEqual = false;
            int limit = 250000;
            int countLoops = 0;

            SHA1Managed sha1Hash = new SHA1Managed();
            this.SendGuiLogMsg("Signature tests started", NotificationLevel.Info);

            while (!isEqual && (countLoops < limit))
            {
                //byte[] hashDigest = Hashfunction.generateHashDigest(message, hashFuncIdent); // Hashwert wird erzeugt
                //byte[] hashDigest = Hashfunction.generateHashDigest(bMessage, hashFuncIdent); // Hashwert wird erzeugt
                byte[] hashDigest = sha1Hash.ComputeHash(bMessage);
                Array.Copy(hashDigest, 0, A, 11 + hashIdent.Length, hashDigest.Length); // erzeugter Hashwert wird in den Datenblock kopiert
                Array.Copy(A, 0, S, 0, A.Length); // erzeugter Datenblock wird in erzeugte Signatur S kopiert

                //string test = Encoding.ASCII.GetString(Hex.Encode(S));
                /*
                ///////////////////////////////////////////////////////////////////////////////////////////
                // nur testweise
                string test1 = Encoding.ASCII.GetString(Hex.Encode(S)); 
                BigInteger fakegarbage = BigInteger.ValueOf(125);
                Array.Copy(fakegarbage.ToByteArray(), 0, S, A.Length, fakegarbage.ToByteArray().Length);
                string test2 = Encoding.ASCII.GetString(Hex.Encode(S));
                //
                //////////////////////////////////////////////////////////////////////////////////////////
                */

                finalSignature = MathFunctions.cuberoot2(new BigInteger(S)); // Kubikwurzel ziehen
                BigInteger T = finalSignature.Pow(3); // mit 3 potenzieren
                byte[] resultArray = new byte[128]; // damit verglichen werden kann in byte array kopieren

                // durch Konvertierung in BigInteger werden führende Nullen abgeschnitten, 
                // daher wird an Stelle 1 in byte array kopiert.
                Array.Copy(T.ToByteArray(), 0, resultArray, 1, T.ToByteArray().Length);

                //string test2 = Encoding.ASCII.GetString(Hex.Encode(resultArray));
                

                isEqual = MathFunctions.compareByteArray(resultArray,S,45); // byte arrays vergleichen, wird in meinen Tests nicht erreicht
                if (!isEqual)
                {
                    
                    byte[] tmp1 = { bMessage[iMsgLength - 3], bMessage[iMsgLength - 2], bMessage[iMsgLength - 1] };

                    BigInteger tmp2 = new BigInteger(tmp1);
                    tmp2 = tmp2.Add(BigInteger.One);
                    tmp1 = tmp2.ToByteArray();

                    Array.Copy( tmp1, 0, bMessage, bMessage.Length-3,3);
                    countLoops++;
                }
            }

            Datablock.getInstance().Message = message;

            byte[] returnByteArray = new byte[this.m_KeyLength/8];
            Array.Copy(finalSignature.ToByteArray(), 0, returnByteArray, returnByteArray.Length - finalSignature.ToByteArray().Length, finalSignature.ToByteArray().Length);

            this.m_Signature = returnByteArray;
            this.m_bSigGenerated = true;
            this.OnRaiseSigGenEvent(SignatureType.Kuehn);
        }

        public event GuiLogHandler OnGuiLogMsgSend;

        public void registerHandOff()
        {
            GuiLogMsgHandOff.getInstance().registerAt(ref OnGuiLogMsgSend);
        }

        public void SendGuiLogMsg(string message, NotificationLevel logLevel)
        {
            if (null != OnGuiLogMsgSend)
            {
                OnGuiLogMsgSend(message, logLevel);
            }
        }
    }
}
