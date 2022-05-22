using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AES.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var conteudoUsuario = "DESENVOLVIMENTO!";

            var chaveEntrada = "20,1,94,33,199,0,48,9,31,94,112,40,59,30,100,248";

            var aesAlgorithm = new AlgoritmoAES(new Chave(chaveEntrada));
            aesAlgorithm.Criptografar(new ConteudoCifrar(Encoding.ASCII.GetBytes(conteudoUsuario)), "cripto");
        }
        
    }    
}

