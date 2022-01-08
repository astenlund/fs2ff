
namespace fs2ff.Models
{
    public class Gdl90StratuxHeartbeat : Gdl90Base
    {
        /// <summary>
        /// "Stratux" GDL90 message (1Hz) 2 bytes
        /// </summary>
        public Gdl90StratuxHeartbeat() : base(2)
        {
            Msg[0] = 0xCC; // Message type "Stratux".
            Msg[1] = 0;
            
            if (ViewModelLocator.Main.DataPositionEnabled)
            {
                Msg[1] = 0x02;
            }

            if (ViewModelLocator.Main.DataAttitudeEnabled)
            {
                Msg[1] |= 0x01;
            }

            Msg[1] |= 1 << 2;
        }
    }
}
