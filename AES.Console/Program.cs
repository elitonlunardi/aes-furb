using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AES
{
    class Program
    {
        public static void Main(string[] args)
        {
            // var chaveEntrada = "20,1,94,33,199,0,48,9,31,94,112,40,59,30,100,248";
            
            // "ABCDEFGHIJKLMNOP";
            var chaveEntrada = "65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80";
            
            var chave = new Chave(chaveEntrada);
            var roundKey00 = chave.ObterTodasPalavras();
            
            Console.WriteLine("Round key: 0");
            LogarRoundKey(roundKey00.ToList());
            Console.WriteLine("");
            
            GerarRoundKeys(roundKey00.ToList());
        }


        private static void GerarRoundKeys(IList<byte[]> expansaoChave)
        {
            IList<List<byte[]>> keySchedule = new List<List<byte[]>>(11)
            {
                expansaoChave.ToList()
            };
            
            for (int indexRoundKeyQueEstaSendoGerada = 1; indexRoundKeyQueEstaSendoGerada < 11; indexRoundKeyQueEstaSendoGerada++)
            {
                Console.WriteLine($"---------------------Round Key #{indexRoundKeyQueEstaSendoGerada}--------------------------------");
                
                var roundKeyAnterior = keySchedule[indexRoundKeyQueEstaSendoGerada - 1];
                var ultimaWordRoundKeyAnterior = roundKeyAnterior[3];
                var primeiraWordRoundKeyAnterior = roundKeyAnterior[0];
                
                var roundKeyNova = new List<byte[]> 
                {
                    GerarPrimeiraPalavraRoundKey(ultimaWordRoundKeyAnterior, primeiraWordRoundKeyAnterior, indexRoundKeyQueEstaSendoGerada) 
                };
                
                roundKeyNova.Add(XorBytes(roundKeyAnterior[1], roundKeyNova[0]));
                roundKeyNova.Add(XorBytes(roundKeyAnterior[2], roundKeyNova[1]));
                roundKeyNova.Add(XorBytes(roundKeyAnterior[3], roundKeyNova[2]));
                LogarRoundKey(roundKeyNova);
                Console.WriteLine($"---------------------Fim da round Key #{indexRoundKeyQueEstaSendoGerada}--------------------------------");
                keySchedule.Add(roundKeyNova);
            }
        }

        private static byte[] GerarPrimeiraPalavraRoundKey(byte[] ultimaWordRoundKeyAnterior, byte[] primeiraWordRoundKeyAnterior, int indexRoundKeyQueEstaSendoGerada)
        {
            Console.WriteLine("Etapas para geração da primeira word");
            Console.WriteLine("");

            Console.WriteLine($"1) Cópia da última palavra da roundkey anterior: {Convert.ToHexString(ultimaWordRoundKeyAnterior)}");
            
            var rotWord = RotWord(ultimaWordRoundKeyAnterior);
            Console.WriteLine($"2) Rotacionar os bytes desta palavra (RotWord): {Convert.ToHexString(rotWord)}; ");
            
            var subWord = SubWord(rotWord);
            Console.WriteLine($"3) Substituir os bytes da palavra (SubWord): {Convert.ToHexString(subWord)}");
            
            var roundConstant = ObterRoundConstant(indexRoundKeyQueEstaSendoGerada);
            Console.WriteLine($"4) Gerar a RoundConstant: {Convert.ToHexString(roundConstant)}");

            var resultadoXorSubWordComRoundConstant = XorBytes(subWord, roundConstant);
            Console.WriteLine($"5) XOR de (3) com (4): {Convert.ToHexString(resultadoXorSubWordComRoundConstant)}");
            
            var primeiraPalavraRoundKey = XorBytes(primeiraWordRoundKeyAnterior, resultadoXorSubWordComRoundConstant);
            Console.WriteLine($"6) XOR 1a. palavra da roundkey anterior com (5): {Convert.ToHexString(primeiraPalavraRoundKey)}");
            
            return primeiraPalavraRoundKey;
        }


        private static byte[]  RotWord(byte[] word)
        {
            var resultado = new byte[4];
            word.CopyTo(resultado,0);
            
            var primeiroByte = word[0];
            for (int i = 0; i < word.Length - 1; i++)
            {
                resultado[i] = word[i + 1];
            }
            resultado[word.Length - 1] = primeiroByte;
            return resultado;
        }
        
        private static byte[] SubWord(byte[] rotWord)
        {
            Console.WriteLine($"Entrada SubWord - HEX: {Convert.ToHexString(rotWord)}");
            Span<byte> subWord = new byte[4];
            for (int i = 0; i < rotWord.Length; i++)
            {
                var byteIteradoDaRotWord = rotWord[i];
                Span<byte> spanByteRotWord = new byte[]{ byteIteradoDaRotWord };
                var byteHexa = Convert.ToHexString(spanByteRotWord);

                var intQuatroBitsMaisSignificativos = int.Parse(byteHexa[0].ToString(), NumberStyles.HexNumber);
                var intQuatroBitsMenosSignificativos = int.Parse(byteHexa[1].ToString(), NumberStyles.HexNumber);

                var byteParaSubstituir = Constantes.TableSBox[intQuatroBitsMaisSignificativos, intQuatroBitsMenosSignificativos];
                subWord[i] = byteParaSubstituir;
            }

            var subWordArray = subWord.ToArray();
            Console.WriteLine($"Saída SubWord - HEX: {Convert.ToHexString(subWordArray)}");
            return subWordArray;
        }

        private static byte[] ObterRoundConstant(int roundKeyQueEstaSendoGerada)
        {
            //index da roundKey começa em 1
            roundKeyQueEstaSendoGerada--;
            var roundConstant = new byte[4]
            {
                Constantes.TableRoundConstant[roundKeyQueEstaSendoGerada],
                0,
                0,
                0
            };
            Console.WriteLine($"Resultado roundConstant #{roundKeyQueEstaSendoGerada} - HEX: {Convert.ToHexString(roundConstant)}");
            return roundConstant;
        }
        
        private static byte[] XorBytes(byte[] a, byte[]b)
        {
            var resultado = new byte[4];
            for (var i = 0; i < a.Length; i++)
            {
                resultado[i] = (byte)(a[i] ^ b[i]);
            }
            return resultado;
        }

        private static void LogarRoundKey(List<byte[]> roundKey)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Console.Write($" {Convert.ToHexString(new []{roundKey[j][i]})}");
                }
                Console.WriteLine("");
            }
        }
    }    
}

