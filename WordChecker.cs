using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace murmur3_Unhash
{
    public class WordChecker
    {

        enum last_state { 
            space = 0, // spaces may also chain
            number = 1, // numbers allow us to chain
            plural = 2, // plurals do not
            none = 3,
        }
        public static float CheckWord(string word){
            char last_char = '_';
            int repeat_count = 0;
            int correct_chars = 0;
            int number_chains = 0;
            // replace ending numbers, y's & s's with underscores
            //StringBuilder new_string = new StringBuilder(word);
            last_state last_char_state = last_state.space;
            for (int i = word.Length - 1; i >= 0; i--){
                bool is_starting_char = (i == 0);
                char current_char = word[i];
                switch (current_char){
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (is_starting_char) // numbers cant be the starting character
                            current_char = '\0';
                        else if (last_char_state <= last_state.number) {
                            if (last_char_state == last_state.space)
                                number_chains++;
                            current_char = '_';
                            last_char_state = last_state.number;
                        } else last_char_state = last_state.none;
                        // we dont want results with more than 1 chain of numbers
                        if (number_chains > 1)
                            current_char = '\0';
                        break;
                    case 's':
                    case 'y':
                        if (last_char_state == last_state.space){
                            current_char = '_';
                            last_char_state = last_state.plural;
                        } else last_char_state = last_state.none;
                        break;
                    case '_': // space character
                        if (is_starting_char) // spaces also cant be starting numbers
                            current_char = '\0';
                        last_char_state = last_state.space;
                        break;
                    default:
                        last_char_state = last_state.none;
                        break;
                }
                // now process this char
                if (EvaluateLetter(current_char, last_char, is_starting_char))
                    correct_chars++;
                if (last_char == current_char && current_char != '\0' && last_char_state != last_state.number && last_char_state != last_state.plural)
                    repeat_count++;
                else repeat_count = 0;
                if (repeat_count == 2 || (current_char == '_' && repeat_count == 1))
                    correct_chars -= 3; // 3 chars in a row means they're all wrong // might as well disqualify the string


                last_char = current_char;
            }
            return (float)correct_chars / word.Length;
        }
        private static bool EvaluateLetter(char letter, char next, bool is_first_char){
            if (letter == '\0')
                return false;
            if (next == '_')
                return true;

            // evaluate for if the current letter is a vowel
            // vowels use exclude logic, as they're otherwise compatible with every other letter
            switch (letter){
                case '_':
                    return true; // theres not much we can do here, aside from figure that its correct
                case 'a':
                    switch (next){
                        case 'a':
                        case 'e':
                        case 'o':
                            return false;
                    } return true;
                case 'e':
                    switch (next){
                        case 'i':
                        case 'o':
                        case 'u':
                            return false;
                    } return true;
                case 'i':
                    switch (next){
                        case 'i':
                        case 'u':
                            return false;
                    } return true;
                case 'o':
                    return next != 'a';
                case 'u':
                    switch (next){
                        case 'i':
                        case 'o':
                        case 'u':
                            return false;
                    } return true;
                // q has to be handled a little differently, as it is the only letter with a vowel restriction (of 'u')
                case 'q':
                    return next == 'u'; // this works becuase q cannot be at the end of a word
                // we dont allow numbers, unless they are before a space (at the end of a word)
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return false;
            }
            // test whether the next character is a natural conjunction piece (vowels + null + space)
            switch (next){
                // if its a number, we cant allow vowels to give it a freebie
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                    return true;
            }
            // now test regular letters for compatibilty with other regular letters
            switch (letter){
                case 'b':
                    return next == 'l';
                case 'c':
                    switch (next){
                        case 'h':
                        case 'l':
                        case 'r':
                            return true;
                        case 'k':
                        case 'q':
                        case 't':
                            return !is_first_char;
                    } return false;
                case 'd':
                    switch (next){
                        case 'r':
                            return true;
                        case 'd':
                            return !is_first_char;
                    } return false;
                case 'f':
                    switch (next){
                        case 'l':
                        case 'r':
                            return true;
                        case 'f':
                        case 't':
                            return !is_first_char;
                    } return false;
                case 'g':
                    switch (next){
                        case 'l':
                        case 'n':
                        case 'r':
                            return true;
                        case 'h':
                        case 'g':
                            return !is_first_char;
                    } return false;
                case 'h':
                    return next == 'y';
                case 'k':
                    return !is_first_char && next == 'l';
                case 'l':
                    switch (next){
                        case 'l':
                        case 'v':
                            return !is_first_char;
                    } return false;
                case 'm':
                    switch (next){
                        case 'm':
                        case 'n':
                        case 'b':
                        case 'p':
                            return !is_first_char;
                    } return false;
                case 'n':
                    switch (next){
                        case 'n':
                        case 'g':
                        case 'c':
                        case 'd':
                        case 'k':
                            return !is_first_char;
                    } return false;
                case 'p':
                    switch (next){
                        case 'l':
                        case 'h':
                            return true;
                        case 'p':
                            return !is_first_char;
                    } return false;
                case 'r':
                    switch (next){
                        case 't':
                        case 'c':
                        case 'r':
                        case 'v':
                        case 's':
                            return !is_first_char;
                    } return false;
                case 's':
                    switch (next){
                        case 't':
                        case 'h':
                        case 'l':
                        case 'q':
                        case 'p':
                            return true;
                        case 's':
                            return !is_first_char;
                    } return false;
                case 't':
                    switch (next){
                        case 'h':
                        case 'r':
                            return true;
                        case 'c':
                            return !is_first_char;
                    } return false;
                case 'x':
                    switch (next){
                        case 'p':
                        case 'c':
                            return !is_first_char;
                    } return false;
                case 'y':
                    switch (next){
                        case 'l':
                        case 's':
                            return !is_first_char;
                    } return false;
            }

            return false; // this shouldn't be reached, unless some foriegn character slipped in
        }

    }
}
