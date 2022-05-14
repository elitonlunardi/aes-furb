using System.Collections;
using System.Collections.Generic;

namespace AES;

public interface IChave
{
    byte[,] Palavras { get;}
    public IEnumerable<byte[]> ObterTodasPalavras();
}