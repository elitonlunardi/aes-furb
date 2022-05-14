using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AES;

public class Chave : IChave
{
    public IList<byte> Composicao { get; private set; }
    public byte[,] Palavras { get; private set; }
    
    public Chave(string chave)
    {
        Composicao = new List<byte>();
        ConverterStringEntradaParaComposicaoEmByte(chave);
        CriarMatrizEstado();
    }

    public IEnumerable<byte[]> ObterTodasPalavras()
    {
        var palavras = new List<byte[]>(4);

        for (int j = 0; j < 4; j++)
        {
            var word = new byte[4];
            for (int i = 0; i < 4; i++)
            { 
                word[i] = Palavras[i, j];
            }
            palavras.Add(word);
        }
        return palavras;
    }

    private void ConverterStringEntradaParaComposicaoEmByte(string entrada)
    {
        try
        {
            Composicao = entrada.Split(',')
                .Select(w => Convert.ToByte(w))
                .ToList();
                 
            var hex = Convert.ToHexString(Composicao.ToArray());

            Console.WriteLine($"Chave de entrada - HEX: {hex}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Entrada inválida. {e.Message}");
        }
    }

    private void CriarMatrizEstado()
    {
        Palavras = new byte[4, 4];

        var word1 = Composicao.Take(new Range(0, 4)).ToList();
        var word2 = Composicao.Take(new Range(4, 8)).ToList();
        var word3 = Composicao.Take(new Range(8, 12)).ToList();
        var word4 = Composicao.Take(new Range(12, 16)).ToList();

        for (int i = 0; i < word1.Count; i++)
        {
            Palavras[i, 0] = word1[i];
        }

        for (int i = 0; i < word2.Count; i++)
        {
            Palavras[i, 1] = word2[i];
        }

        for (int i = 0; i < word3.Count; i++)
        {
            Palavras[i, 2] = word3[i];
        }

        for (int i = 0; i < word4.Count; i++)
        {
            Palavras[i, 3] = word4[i];
        }
    }
}