using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AES;

public class ConteudoCifrar
{
    public byte[,] MatrizEstado { get; private set; }
    public IList<byte[,]> Blocos { get; private set; }

    public ConteudoCifrar(byte[] entrada)
    {
        int quantidadeBlocos = entrada.Length / 16;
        Blocos = new List<byte[,]>(quantidadeBlocos);

        var blocosTemp = new List<byte?[,]>();

        var novoBloco = new byte?[4, 4];

        var indexColunaPalavraAtualNoBloco = 0;
        var indexLinhaPalavraAtualNoBloco = 0;

        for (int i = 0; i < entrada.Length; i++)
        {
            novoBloco[indexLinhaPalavraAtualNoBloco, indexColunaPalavraAtualNoBloco] = entrada[i];
            indexLinhaPalavraAtualNoBloco++;
            if (indexColunaPalavraAtualNoBloco == 3 && indexLinhaPalavraAtualNoBloco == 4 || i == entrada.Length - 1)
            {
                blocosTemp.Add(novoBloco);
                novoBloco = new byte?[4, 4];
                indexColunaPalavraAtualNoBloco = 0;
                indexLinhaPalavraAtualNoBloco = 0;
            }

            if (indexLinhaPalavraAtualNoBloco == 4)
            {
                indexColunaPalavraAtualNoBloco++;
                indexLinhaPalavraAtualNoBloco = 0;
            }
        }

        var valorPreenchimentoPadding = 0;
        for (int i = 0; i < blocosTemp[^1].GetLength(0); i++)
        {
            for (int j = 0; j < blocosTemp[^1].GetLength(1); j++)
            {
                if (blocosTemp[^1][j,i] == null)
                {
                    valorPreenchimentoPadding++;
                }
            }
        }
        
        for (int i = 0; i < blocosTemp[^1].GetLength(0); i++)
        {
            for (int j = 0; j < blocosTemp[^1].GetLength(1); j++)
            {
                if (blocosTemp[^1][j,i] == null)
                {
                    blocosTemp[^1][j, i] = (byte)valorPreenchimentoPadding;
                }
            }
        }

        for (int i = 0; i < blocosTemp.Count; i++)
        {
            var blocoConvertido = new byte[4, 4];

            for (int j = 0; j < blocosTemp[i].GetLength(0); j++)
            {
                for (int k = 0; k <  blocosTemp[i].GetLength(1); k++)
                {
                    blocoConvertido[k, j] = blocosTemp[i][k, j].Value;
                }
            }
            Blocos.Add(blocoConvertido);
        }
        
        if (entrada.Length % 16 == 0)
        {
            Blocos.Add(BlocoInteiroPadding16Bytes());
        }
    }

    private byte[,] BlocoInteiroPadding16Bytes() => new byte[,]
    {
        { 16, 16, 16, 16 },
        { 16, 16, 16, 16 },
        { 16, 16, 16, 16 },
        { 16, 16, 16, 16 }
    };
}