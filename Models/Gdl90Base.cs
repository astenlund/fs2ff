using System.Runtime.InteropServices;

namespace fs2ff.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class Gdl90Base
    {
        protected byte[] Msg;

        protected Gdl90Base(int size)
        {
            Msg = new byte[size];
        }

        public byte[] ToGdl90Message()
        {
            return Msg.MakeGdl90Message();
        }
    }
}
