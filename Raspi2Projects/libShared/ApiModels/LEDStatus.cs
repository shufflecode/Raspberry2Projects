
namespace libShared.ApiModels
{
    class LEDStatus
    {
       
        public int LedNumber;

        public Status status;
        public enum Status
        {
            on=1,
            off=2
        }
    }
}