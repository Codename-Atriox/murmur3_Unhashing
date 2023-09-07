using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace murmur3_Unhash
{
    internal class Program
    {
        struct sorted_things{
            public sorted_things(float a, int i){
                accuracy = a; string_index = i;}
            public float accuracy;
            public int string_index; 
        }
        static void Main(string[] args)
        {

            while (true)
            {
                Console.WriteLine("enter string to test...");
                string? new_string = Console.ReadLine();
                if (new_string == null) continue;

                uint test = Murmur3.HashString(new_string);


                List<string> matching_strings = Murmur3.unhash(test, (uint)new_string.Length);
                Console.WriteLine(matching_strings.Count + " matching strings found!");
                Console.ReadLine();

                // now we apply our word checking logic & then sort
                List<sorted_things> sorted = new(matching_strings.Count);

                for (int i = 0; i < matching_strings.Count; i++)
                    sorted.Add(new sorted_things(WordChecker.CheckWord(matching_strings[i], true, false), i));
                
                sorted.Sort((a, b) => b.accuracy.CompareTo(a.accuracy));

                int num_displayed = 0;
                foreach(var v in sorted){
                    if (num_displayed > 1000){
                        Console.WriteLine("display fulling up!!! enter to continue!");
                        Console.ReadLine();
                        num_displayed = 0;
                    }

                    Console.WriteLine("[" + v.accuracy.ToString("0.0000") + "]:" + matching_strings[v.string_index]);
                    num_displayed++;
                }

            }
            //Random r_gen = new();
            //uint test = (uint)r_gen.Next();


            // testing fmix reversal: SUCCESS
            //for (uint test = 0; test < 0xffffffff; test++)
            //{
            //    uint test_hashed = fmix(test);
            //    uint test_unhased = unfmix(test_hashed);
            //    if (test != test_unhased)
            //        Console.WriteLine("og: " + test.ToString("X8") + " hashed: " + test_hashed.ToString("X8") + " unhashed: " + test_unhased.ToString("X8"));
            //}
            //Console.WriteLine("finished fmix test!");
            //Console.ReadLine();

            // testing multiplication reversal: SUCCESS
            // source: https://pari.math.u-bordeaux.fr/gp.html
            // formula: 1/Mod(x, 2^32), where x = uint to find reverse of
            //for (uint i = 0; i < 0xffffffff; i++)
            //{
            //    uint hashed = i * 5; // inverse: 0xCCCCCCCD
            //    //uint hashed = h * 0x85ebca6b; // inverse: 0xA5CB9243
            //    //uint hashed = h * 0xc2b2ae35; // inverse: 0x7ED1B41D
            //    uint reversed_h = hashed * 0xCCCCCCCD;
            //    if (reversed_h != i)
            //        Console.WriteLine("og: " + i.ToString("X8") + " hashed: " + hashed + " unhashed: " + reversed_h.ToString("X8"));
            //}
            //Console.WriteLine("finished inverse multiplication test!");
            //Console.ReadLine();



        }






        
    }

    class Murmur3Reversal{

    }
    static class Murmur3 {

        // //////////////// //
        // REVERSING STUFF //
        // ////////////// //
        public static List<string> unhash(uint hash, uint unhashed_length){
            if (unhashed_length <= 0) throw new Exception("Unhashed length must be greater than 0");
            if (!initialized) cache_hashes();

            hash = unfmix(hash);
            hash ^= unhashed_length;

            // if the length is 4 or less, then the answer lies within our cached hash table
            // if its just 4, we let that get handled down the bottom
            // TODO: add count initializer based off of approximations
            List<string> strings = new();
            string? result;
            switch (unhashed_length){
                case 1:
                    if (chars1.TryGetValue(hash, out result)) strings.Add(result);
                    return strings;
                case 2:
                    if (chars2.TryGetValue(hash, out result)) strings.Add(result);
                    return strings;
                case 3:
                    if (chars3.TryGetValue(hash, out result)) strings.Add(result);
                    return strings;
            }
            // else we have more than 4chars & require a recursive search
            // NOTE: we have to read the smallest block here, not in the recursive part
            uint bytes_in_this_block = unhashed_length % 4; // get however many odd bytes there are
            switch (bytes_in_this_block){
                case 1:
                    foreach (var v in chars1) 
                        recurs_unhash(hash ^ v.Key, unhashed_length - bytes_in_this_block, strings, v.Value);
                    return strings;
                case 2:
                    foreach (var v in chars2) 
                        recurs_unhash(hash ^ v.Key, unhashed_length - bytes_in_this_block, strings, v.Value);
                    return strings;
                case 3:
                    foreach (var v in chars3) recurs_unhash(hash ^ v.Key, unhashed_length - bytes_in_this_block, strings, v.Value);
                    return strings;
            }

            recurs_unhash(hash, unhashed_length, strings, "");
            return strings; 
        }
        // s_list should be a static. not passed down to each node
        private static void recurs_unhash(uint hash, uint length_remaining, List<string> s_list, string hierarchy){
            if (length_remaining < 4) throw new Exception("bad string length!!");
            if (length_remaining == 4){
                hash = unpack4chars(hash);
                string? result;
                if (chars4.TryGetValue(hash, out result)) s_list.Add(result + hierarchy);
                return;}
            // if more than 4, continue the recurse chain
            hash = unpack4chars(hash);
            // we have now optimized this, so we only run the good combinations, opposed to all of them
            if (hierarchy.Length > 0){
                // get the last char in our current string
                char compat_test = hierarchy[0];
                foreach (var v in char_compats[compat_test])
                    recurs_unhash(hash ^ v, length_remaining - 4, s_list, chars4[v] + hierarchy);
            }
            // since we dont know what this is supposed to be, we'll have to test every single combination
            else foreach (var v in chars4) recurs_unhash(hash ^ v.Key, length_remaining-4, s_list, v.Value + hierarchy);
        }
        private static uint unpack4chars(uint hash){
            // if the value is less than constant3, then it must have originally overflowed
            if (hash < c3) return unpack4chars_((ulong)hash + 0x100000000); 
            else return unpack4chars_(hash);
        }
        private static uint unpack4chars_(ulong input) {
            input -= c3;
            if (input >= 0x100000000) throw new Exception("logic exception!! number must fall into a uint's range");
            input *= 0xCCCCCCCD;
            return unrotl32((uint)input, 13);
        }

        const string wide_charset = "abcdefghijklmonpqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_. ";
        const string strict_charset = "abcdefghijklmonpqrstuvwxyz0123456789_";
        static bool initialized = false;
        static Dictionary<uint, string> chars4 = new(162203); // 1874161 // 162203 // 190642 // 229381
        static Dictionary<uint, string> chars3 = new(  9535); //   50653 //   9535 //  11575 //  11575
        static Dictionary<uint, string> chars2 = new(   540); //    1369 //    540 //    693 //    693
        static Dictionary<uint, string> chars1 = new(    37); //      37 //     37 //     37 //     35

        static Dictionary<uint, List<uint>> char_compats = new(37); // compatibility backwards, so `WordChecker.CheckWord(chars4[char_compats[i]] + strict_charset[i]);`
        public static void cache_hashes(){
            // iterate through all chars & pack them
            // updated version
            foreach (char c1 in strict_charset){ 
                if (c1 != 'q' && c1 != '_') // 1 char means end of string, cannot end with space or q
                    chars1.Add(pack32(c1), c1.ToString());
                foreach (char c2 in strict_charset){
                    string s2 = "" + c1 + c2;
                    if (WordChecker.CheckWord(s2, false, false) == 1.0f) // 2 chars means end of string, cannot end with space or q
                        chars2.Add(pack32((uint)(c1 | c2 << 8)), s2);
                    foreach (char c3 in strict_charset){
                        string s3 = "" + c1 + c2 + c3;
                        if (WordChecker.CheckWord(s3, false, false) == 1.0f) // 3 chars means end of string, cannot end with space or q
                            chars3.Add(pack32((uint)(c1 | c2 << 8 | c3 << 16)), s3);
                        foreach (char c4 in strict_charset){
                            string s4 = "" + c1 + c2 + c3 + c4;
                            if (WordChecker.CheckWord(s4, false, true) == 1.0f) // 4 chars means maybe not end of string, can end with space or q
                                chars4.Add(pack32((uint)(c1 | c2 << 8 | c3 << 16 | c4 << 24)), s4);
            }}}}
            // then build a compatibility table
            foreach (char c in strict_charset){
                List<uint> compatible_blocks = new();
                foreach (var v in chars4){
                    if (WordChecker.CheckWord(v.Value + c, false, true) == 1.0f)
                        // then this is compatible, pass it
                        compatible_blocks.Add(v.Key);
                }
                char_compats.Add(c, compatible_blocks);
            }

            //foreach (char c1 in strict_charset){
            //    chars1.Add(pack32(c1), c1.ToString());
            //    foreach (char c2 in strict_charset){
            //        chars2.Add(pack32((uint)(c1 | c2 << 8)), c1.ToString() + c2.ToString());
            //        foreach (char c3 in strict_charset){
            //            chars3.Add(pack32((uint)(c1 | c2 << 8 | c3 << 16)), c1.ToString() + c2.ToString() + c3.ToString());
            //            foreach (char c4 in strict_charset){
            //                chars4.Add(pack32((uint)(c1 | c2 << 8 | c3 << 16 | c4 << 24)), c1.ToString() + c2.ToString() + c3.ToString() + c4.ToString());
            //}}}}
            initialized = true; // 1094ms, 1187ms // 530ms
                                                // ???? you think it'd be slowed now that we're checking every the validness of ever string
        }


        // ////////////// //
        // HASHING STUFF //
        // //////////// //
        public static uint HashString(string hash_in){
            using (MemoryStream stream = new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(hash_in)))
                return Hash(stream);
        }
        const uint seed = 0;
        const uint c1 = 0xcc9e2d51;
        const uint c2 = 0x1b873593;
        const uint c3 = 0xe6546b64;
        private static uint Hash(Stream stream) {
            uint h1 = seed;
            uint streamLength = 0;
            using (BinaryReader reader = new BinaryReader(stream)) {
                byte[] chunk = reader.ReadBytes(4);
                while (chunk.Length > 0) {
                    streamLength += (uint)chunk.Length;
                    switch (chunk.Length) {
                    case 4:
                        h1 ^= pack32((uint)(chunk[0] | chunk[1] << 8 | chunk[2] << 16 | chunk[3] << 24));
                        // the other stuff that is done for 4bytes
                        h1 = rotl32(h1, 13);
                        h1 = h1 * 5 + c3;
                        break;
                    case 3:
                        h1 ^= pack32((uint)(chunk[0] | chunk[1] << 8 | chunk[2] << 16));
                        break;
                    case 2:
                        h1 ^= pack32((uint)(chunk[0] | chunk[1] << 8));
                        break;
                    case 1:
                        h1 ^= pack32(chunk[0]);
                        break;
                    }
                    chunk = reader.ReadBytes(4);
            }}

            // the other hashing stuff
            h1 ^= streamLength;
            h1 = fmix(h1);

            return h1;
        }
        private static uint pack32(uint input){
            input *= c1;
            input = rotl32(input, 15);
            input *= c2;
            return input;
        }

        private static uint rotl32(uint x, byte r){
            return (x << r) | (x >> (32 - r));
        }
        private static uint unrotl32(uint x, byte r){ // rotates in inverse direction
            return (x << (32 - r)) | (x >> r);
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
        static uint reverse_xorshift(uint input, int shift){
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