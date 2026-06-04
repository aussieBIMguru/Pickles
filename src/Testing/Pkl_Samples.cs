namespace Pkl_Testing
{
    public class Pkl_Samples
    {
        //The empty private constructor.
        //This will be not imported into Dynamo.
        private Pkl_Samples() { }

        //The public multiplication method. 
        //This will be imported into Dynamo.
        public static double MultiplyByTwo(double inputNumber)
        {
            return inputNumber * 2.0;
        }
    }
}