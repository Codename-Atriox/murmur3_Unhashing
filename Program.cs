namespace murmur3_Unhash
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");


            Random r_gen = new();
            uint test = (uint)r_gen.Next();
            uint test_hashed = fmix(test);
            uint test_unhased = unfmix(test);





            // testing multiplication reversal
            // source: https://pari.math.u-bordeaux.fr/gp.html
            // formula: 1/Mod(x, 2^32), where x = uint to find reverse of
            for (uint i = 0; i < 0xffffffff; i++){
                //Random r_gen = new();
                //uint h = (uint)r_gen.Next();
                uint h = i;

                uint hashed = h * 5; // inverse: 0xCCCCCCCD
                //uint hashed = h * 0x85ebca6b; // inverse: 0xA5CB9243
                //uint hashed = h * 0xc2b2ae35; // inverse: 0x7ED1B41D


                uint reversed_h = hashed * 0xCCCCCCCD;
                if (reversed_h != h)
                    Console.WriteLine("og: " + h.ToString("X8") + " hashed: " + hashed + " unhashed: " + reversed_h.ToString("X8"));

                //Console.ReadLine();
            }



        }
        private static uint fmix(uint input){
            input ^= input >> 16;
            input *= 0x85ebca6b;
            input ^= input >> 13;
            input *= 0xc2b2ae35;
            input ^= input >> 16; 
            return input;
        }
        private static uint unfmix(uint input){
            // no fancy stuff needed to reverse this
            input ^= input >> 16;
            // multiply inverse (of 0xc2b2ae35)
            input *= 0x7ED1B41D;
            // do a special xorshift to reverse
            input = reverse_xorshift(input, 13);
            // multiply inverse (of 0x85ebca6b)
            input *= 0xA5CB9243; 
            // also no fancy stuff needed
            input ^= input >> 16;
            return input;
        }
        static uint reverse_xorshift(uint input, int shift)
        {
            // get the lower end that originally gets shifted & XOR'd into the input
            uint lower_component = input >> shift;
            // we then account for overlapping bits by calling another xor on them
            uint overlapping_bits = lower_component >> shift;
            // how would we account for if the bitcount was higher than 16?
            // xor all the componenets together
            return input ^ lower_component ^ overlapping_bits;
        }
    }


    
}