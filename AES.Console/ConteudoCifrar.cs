using System;
using System.Collections.Generic;
using System.Linq;
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

        var novoBloco = new byte[4, 4];

        var indexColunaPalavraAtualNoBloco = 0;
        var indexLinhaPalavraAtualNoBloco = 0;
        for (int i = 0; i < entrada.Length; i++)
        {
            novoBloco[indexLinhaPalavraAtualNoBloco, indexColunaPalavraAtualNoBloco] = entrada[i];
            indexLinhaPalavraAtualNoBloco++;
            if (indexColunaPalavraAtualNoBloco == 3 && indexLinhaPalavraAtualNoBloco == 4)
            {
                Blocos.Add(novoBloco);
                novoBloco = new byte[4, 4];
                indexColunaPalavraAtualNoBloco = 0;
                indexLinhaPalavraAtualNoBloco = 0;
            }

            if (indexLinhaPalavraAtualNoBloco == 4)
            {
                indexColunaPalavraAtualNoBloco++;
                indexLinhaPalavraAtualNoBloco = 0;
            }
        }
        
        Blocos.Add(BlocoInteiroPadding16Bytes());
    }
    
    private byte[,] BlocoInteiroPadding16Bytes() => new byte[,]
    {
        { 16, 16, 16, 16 },
        { 16, 16, 16, 16 },
        { 16, 16, 16, 16 },
        { 16, 16, 16, 16 }
    };
}