using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AES
{
    class Program
    {
        public static void Main(string[] args)
        {
            var conteudoUsuario = "DESENVOLVIMENTO!";
            var entradaUsuario = new ConteudoCifrar(Encoding.ASCII.GetBytes(conteudoUsuario));
            
            LogarMatrizEstado("", entradaUsuario.Blocos[0]);

            // var chaveEntrada = "20,1,94,33,199,0,48,9,31,94,112,40,59,30,100,248";
            
            // "ABCDEFGHIJKLMNOP";
            var chaveEntrada = "65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80";
            var chave = new Chave(chaveEntrada);
            var roundKey00 = chave.ObterTodasPalavras();
            
            Console.WriteLine("Round key: 0");
            LogarRoundKey(roundKey00.ToList());
            Console.WriteLine("");
            
            var keySchedule = GerarRoundKeys(roundKey00.ToList());
            var matrizEstadoInicial = entradaUsuario.Blocos;

            var primeiroXor = matrizEstadoInicial[0];
            var primeiraRoundKey = keySchedule[0];

            var matrizEstadoResultado = XorBytes(primeiroXor, primeiraRoundKey);
            LogarMatrizEstado("\n --- Xor" ,matrizEstadoResultado);
            
            matrizEstadoResultado = SubBytes(matrizEstadoResultado);
            LogarMatrizEstado("\n --- SubBytes" ,matrizEstadoResultado);
            
            matrizEstadoResultado = ShiftRows(matrizEstadoResultado);
            LogarMatrizEstado("\n --- ShiftRows", matrizEstadoResultado);
            
            matrizEstadoResultado = MixColumns(matrizEstadoResultado);
            LogarMatrizEstado("\n --- MixColumns", matrizEstadoResultado);
            
            matrizEstadoResultado = XorBytes(matrizEstadoResultado, keySchedule[1]);
            LogarMatrizEstado("\n --- XorBytes MixColums + RoundKey", matrizEstadoResultado);
        }


        private static IList<List<byte[]>> GerarRoundKeys(IList<byte[]> expansaoChave)
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

            return keySchedule;
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


        private static byte[] RotWord(byte[] word)
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

        private static byte[,] ShiftRows(byte[,] matrizEstado)
        {
            byte[,] matrizEstadoResultado = (byte[,])matrizEstado.Clone();

            for (int i = 1; i < matrizEstadoResultado.GetLength(0); i++)
            {
                var linhaAtual = new byte[4];
                for (int j = 0; j < linhaAtual.Length; j++)
                {
                    linhaAtual[j] = matrizEstadoResultado[i, j];
                }

                int quantidadeShifts = i;
                for (int j = quantidadeShifts; j > 0; j--)
                {
                    linhaAtual = RotWord(linhaAtual);
                }
                
                for (int k = 0; k < linhaAtual.Length; k++)
                {
                    matrizEstadoResultado[i,k] = linhaAtual[k];
                }
            }
            
            return matrizEstadoResultado;
        }
        
        private static byte[] SubWord(byte[] rotWord)
        {
            Span<byte> subWord = new byte[4];
            for (int i = 0; i < rotWord.Length; i++)
            {
                var byteIteradoDaRotWord = rotWord[i];
                (var intQuatroBitsMaisSignificativos, var intQuatroBitsMenosSignificativos) = ObterBitMaisEMenosSignificativo(byteIteradoDaRotWord);

                var byteParaSubstituir = Constantes.MatrizSBox[intQuatroBitsMaisSignificativos, intQuatroBitsMenosSignificativos];
                subWord[i] = byteParaSubstituir;
            }

            var subWordArray = subWord.ToArray();
            return subWordArray;
        }
        
        private static byte[,] SubBytes(byte[,] matrizEstado)
        {
            var matrizEstadoResultante = new byte[4,4];
            for (int i = 0; i < matrizEstado.GetLength(0); i++)
            {
                for (int j = 0; j < matrizEstado.GetLength(1); j++)
                {
                    var byteIteradoDaMatrizEstado = matrizEstado[i,j];
                    (var intQuatroBitsMaisSignificativos, var intQuatroBitsMenosSignificativos) = ObterBitMaisEMenosSignificativo(byteIteradoDaMatrizEstado);
                    var byteParaSubstituir = Constantes.MatrizSBox[intQuatroBitsMaisSignificativos, intQuatroBitsMenosSignificativos];

                    matrizEstadoResultante[i, j] = byteParaSubstituir;
                }
            }
            return matrizEstadoResultante;
        }

        private static byte[] ObterRoundConstant(int roundKeyQueEstaSendoGerada)
        {
            //index da roundKey começa em 1
            roundKeyQueEstaSendoGerada--;
            var roundConstant = new byte[4]
            {
                Constantes.RoundConstant[roundKeyQueEstaSendoGerada],
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

        private static byte[,] XorBytes(byte[,] matrizEstado, IList<byte[]> roundKeyAtual)
        {
            var matrizEstadoResultado = new byte[4,4];
            for (var i = 0; i < matrizEstado.GetLength(0); i++)
            {
                for (int j = 0; j < matrizEstado.GetLength(1); j++)
                {
                    var wordMomentoKeySchedule = roundKeyAtual[i];
                    var primeiroElemento = matrizEstado[j, i];
                    var segundoElemento = wordMomentoKeySchedule[j];
                    var byteDoMomento = (byte)(primeiroElemento ^ segundoElemento);
                    
                    matrizEstadoResultado[j, i] = byteDoMomento;
                }
            }

            return matrizEstadoResultado;
        }

        private static (int BitMaisSignificativo, int BitMenosSignificativo ) ObterBitMaisEMenosSignificativo(byte byteParaObterBitMaisEMenosSignificativo)
        {
            Span<byte> spanComByte = new byte[]{ byteParaObterBitMaisEMenosSignificativo };
            var byteHexa = Convert.ToHexString(spanComByte);
            
            var intQuatroBitsMaisSignificativos = int.Parse(byteHexa[0].ToString(), NumberStyles.HexNumber);
            var intQuatroBitsMenosSignificativos = int.Parse(byteHexa[1].ToString(), NumberStyles.HexNumber);

            return (intQuatroBitsMaisSignificativos, intQuatroBitsMenosSignificativos);
        }

        private static byte[,] MixColumns(byte[,] matrizEstado)
        {
            byte[,] matrizResultado = new byte[matrizEstado.GetLength(0), matrizEstado.GetLength(1)];

            for (int i = 0; i < matrizEstado.GetLength(0); i++)
            {
                var r1 = matrizEstado[0, i];
                var r2 = matrizEstado[1, i];
                var r3 = matrizEstado[2, i];
                var r4 = matrizEstado[3, i];
                
                matrizResultado[0, i] = (byte)(MultiplicacaoGalois(r1, 2) ^
                                               MultiplicacaoGalois(r2, 3) ^
                                               MultiplicacaoGalois(r3, 1) ^
                                               MultiplicacaoGalois(r4, 1));
                
                
                matrizResultado[1, i] = (byte)(MultiplicacaoGalois(r1, 1) ^
                                               MultiplicacaoGalois(r2, 2) ^
                                               MultiplicacaoGalois(r3, 3) ^
                                               MultiplicacaoGalois(r4, 1));
                
                matrizResultado[2, i] = (byte)(MultiplicacaoGalois(r1, 1) ^
                                               MultiplicacaoGalois(r2, 1) ^
                                               MultiplicacaoGalois(r3, 2) ^
                                               MultiplicacaoGalois(r4, 3));
                
                matrizResultado[3, i] = (byte)(MultiplicacaoGalois(r1, 3) ^
                                               MultiplicacaoGalois(r2, 1) ^
                                               MultiplicacaoGalois(r3, 1) ^
                                               MultiplicacaoGalois(r4, 2));
            }
            
                        
            return matrizResultado;
        }

        private static byte MultiplicacaoGalois(byte operacaoMomento, byte constanteDaTabelaMultiplicacao)
        {
            if (operacaoMomento == 0 || constanteDaTabelaMultiplicacao == 0)
                return 0;

            if (operacaoMomento == 1)
                return constanteDaTabelaMultiplicacao;

            if (constanteDaTabelaMultiplicacao == 1)
                return operacaoMomento;
            
            var (bitMaisSignificativoOperacaoMomento,  bitMenosSignificativoOperacaoMomento) = ObterBitMaisEMenosSignificativo(operacaoMomento);
            var resultadoTabelaLParaOperacaoMomento = Constantes.MatrizL[bitMaisSignificativoOperacaoMomento, bitMenosSignificativoOperacaoMomento];
            
            var (bitMaisSignificativoConstanteDaTabelaMultiplicacao,  bitMenosSignificativoConstanteDaTabelaMultiplicacao) = ObterBitMaisEMenosSignificativo(constanteDaTabelaMultiplicacao);
            var resultadoTabelaLParaConstanteDaTabelaMultiplicacao = Constantes.MatrizL[bitMaisSignificativoConstanteDaTabelaMultiplicacao, bitMenosSignificativoConstanteDaTabelaMultiplicacao];

            var somaDosResultados = Convert.ToInt32(resultadoTabelaLParaOperacaoMomento) + Convert.ToInt32(resultadoTabelaLParaConstanteDaTabelaMultiplicacao);
            if (somaDosResultados > 0xFF)
            {
                somaDosResultados = somaDosResultados - 0xFF;
            }

            var somaDosResultadosBitMaisMenosSignificativo = ObterBitMaisEMenosSignificativo((byte)somaDosResultados);
            return Constantes.MatrizE[somaDosResultadosBitMaisMenosSignificativo.BitMaisSignificativo, somaDosResultadosBitMaisMenosSignificativo.BitMenosSignificativo];
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
        
        private static void LogarMatrizEstado(string headerLog, byte[,] matriz)
        {
            Console.WriteLine(headerLog);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Console.Write($" {Convert.ToHexString(new []{matriz[i,j]})}");
                }
                Console.WriteLine("");
            }
        }
    }    
}

