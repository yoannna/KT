using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReedMuller
{
    class Coder
    {
        //          3  2
        private int m, r;
        private bool[][] matrix;
        private bool[][] dictionary;
        private int[] indexes;
        public bool[][] Matrix => matrix;
        public int WordLenght => matrix.Length;
        public int CodeLenght => matrix[0].Length;

        private static int charSize = 8;
        private static int byteSize = 8;

        public Coder(int m, int r)
        {
            if (m < 0 || r < 0 || r >= m)
                throw new ArgumentException();

            this.m = m;
            this.r = r;
            GenerateMatrix();

            dictionary = new bool[CodeLenght][];
            for (int i = 0; i < CodeLenght; i++)
            {
                dictionary[i] = ToBinary(i, m).ToArray();
            }

            indexes = new int[m];
            for (int i = 1; i<= m; i++)
            {
                indexes[i - 1] = i;
            }
        }
        
        /// <summary>Converts intger value to its binary expresion.</summary>
        /// <param name="value">Number to convert</param>
        /// <param name="length">Binary expresion length</param>
        /// <returns>Binary expresion as bool list</returns>
        private static List<bool> ToBinary(int value, int length)
        {
            List<bool> bin = length > 1 ? ToBinary(value >> 1, length - 1) : new List<bool>();
            bin.Add((value & 1) == 1);
            return bin;
        }

        /// <summary>Calculates factorial of a given number.</summary>
        /// <param name="number">Number to calculate factorial of</param>
        /// <returns>Factorial</returns>
        private int GetFactorial(int number)
        {
            if (number <= 1)
                return 1;
            return number * GetFactorial(number - 1);
        }

        /// <summary>Calculates combinations count.</summary>
        /// <param name="n">Number of elemnts</param>
        /// <param name="r">Combination size</param>
        /// <returns>Combinations count</returns>
        private int GetCombinationsCount(int n, int r)
        {
            int permutationsCount = 1;
            for (int i = n; i > n - r; i--)
                permutationsCount *= i;

            return permutationsCount / GetFactorial(r);
        }

        /// <summary>Calculates all given size combinations in the array.</summary>
        /// <param name="array">Elements to get combinations from</param>
        /// <param name="elementCount">Combination size</param>
        /// <returns>List of combinations</returns>
        public List<int[]> GetCombinations(int[] array, int elementCount)
        {
            if (array.Length < elementCount)
                throw new ArgumentException();
            if (elementCount < 1)
                return new List<int[]> { new int[] { 0 } };

            List<int[]> combinations = new List<int[]>();
            int max = array.Length - elementCount + 1;
            GetCombinations(combinations, array, elementCount, 0, max);

            return combinations;
        }

        /// <summary>Recursive method to calculate all given size combinations in the array.</summary>
        /// <param name="combinations">List to add combinations to</param>
        /// <param name="array">Elements to get combinations from</param>
        /// <param name="elementCount">Combination size</param>
        /// <param name="currentIndex">Index of current combination element</param>
        /// <param name="maxIndex">Index of last element to add</param>
        private void GetCombinations(List<int[]> combinations, int[] array, int elementCount, int currentIndex, int maxIndex, int[] current = null, int[] steps = null)
        {
            if (current == null)
            {
                current = new int[elementCount];
                steps = new int[elementCount];
            }

            for (int i = currentIndex + steps[currentIndex]; i < maxIndex; i++)
            {
                current[currentIndex] = array[i];
                
                if (currentIndex == elementCount - 1)
                {
                    // Can't add reference to changing array
                    int[] copy = new int[elementCount];
                    for (int j = 0; j < elementCount; j++)
                        copy[j] = current[j];
                    
                    combinations.Add(copy);
                }
                else
                {
                    GetCombinations(combinations, array, elementCount, currentIndex + 1, maxIndex + 1, current, steps);
                }

            }

            if (currentIndex > 0)
            {
                steps[currentIndex - 1]++;
                for (int i = currentIndex; i < elementCount; i++)
                    steps[i] = steps[currentIndex - 1];
            }

        }

        /// <summary>Calculates all combinations in the array from size 2 to r.</summary>
        /// <param name="array">Elements to get combinations from</param>
        /// <returns>List of combinations</returns>
        private List<int[]> GetCombinations(int[] array)
        {
            int max = array.Length - 1;
            List<int[]> combinations = new List<int[]>();
            for (int i = 2; i <= r; i++)
            {
                List<int[]> newPairs = GetCombinations(array, i);
                combinations = combinations.Concat(newPairs).ToList();
            }
            return combinations;
        }

        /// <summary>Generates matrix for given r and m.</summary>
        private void GenerateMatrix()
        {
            // Calculate matrix dimensions:
            int strength = 1;
            for (int i = 1; i <= r; i++)
                strength += GetCombinationsCount(m, i);

            int length = 1;
            for (int i = 0; i < m; i++)
                length *= 2;

            // Initiate empty matrix:
            matrix = new bool[strength][];
            for (int i = 0; i < strength; i++)
                matrix[i] = new bool[length];

            // Fill first row:
            for (int i = 0; i < length; i++)
            {
                // default is false
                matrix[0][i] = true;
            }

            // Fill first level rows
            // For example:
            // 1 1 1 1 0 0 0 0
            // 1 1 0 0 1 1 0 0
            // 1 0 1 0 1 0 1 0
            int rowNumber = 1;
            for (int i = length / 2; i >= 1; i /= 2)
            {
                int j = 0;
                while (j < length)
                {
                    for (int k = j; k < j + i; k++)
                    {
                        matrix[rowNumber][k] = true;
                    }
                    j += i;

                    for (int k = j; k < j + i; k++)
                    {
                        matrix[rowNumber][k] = false;
                    }
                    j += i;
                }

                rowNumber++;
            }

            // Fill higher level rows:
            if (r >= 2)
            {
                // Array of row numbers to calculate combinations of:
                int[] rowNumbers = new int[m];
                for (int i = 0; i < m; i++)
                    rowNumbers[i] = i + 1;

                // Fill higher level rows by multiplying first level rows:
                List<int[]> combinations = GetCombinations(rowNumbers);
                foreach (int[] combination in combinations)
                {
                    for (int i = 0; i < length; i++)
                    {
                        bool value = true;
                        foreach (int index in combination)
                        {
                            if (!matrix[index][i])
                            {
                                value = false;
                                break;
                            }
                        }
                        matrix[rowNumber][i] = value;
                    }
                    rowNumber++;
                }
            }
        }

        /// <summary>Encodes given word</summary>
        /// <param name="word">Word to encode</param>
        /// <returns>Encoded word</returns>
        public bool[] Encode(bool[] word)
        {
            if (word.Length != WordLenght)
                throw new ArgumentException();

            bool[] code = new bool[matrix[0].Length];
            // Multiply word by each column in the matrix:
            // For example 3rd element in encoded word is word multiplied by 3rd column in the matrix.
            for (int i = 0; i < matrix[0].Length; i++)
            {
                int matches = 0;
                bool[] column = new bool[WordLenght];

                for (int k = 0; k < WordLenght; k++)
                    column[k] = matrix[k][i];

                for (int j = 0; j < WordLenght; j++)
                    if (word[j] && column[j])
                        matches++;

                if (matches % 2 == 1)
                    code[i] = true;
            }

            return code;
        }
        
        /// <summary>Splits given list to words</summary>
        /// <param name="list">List to split into words</param>
        /// <param name="addedCount">Number of added zeros will be saved in this variable.</param>
        /// <returns>List of words</returns>
        private List<bool[]> SplitToWords(List<bool> list, out int addedCount)
        {
            addedCount = WordLenght - list.Count % WordLenght;
            for (int i = 0; i < addedCount; i++)
            {
                list.Add(false);
            }

            List<bool[]> words = new List<bool[]>();
            int wordCount = list.Count / WordLenght;
            for (int i = 0; i < wordCount; i++)
            {
                bool[] word = new bool[WordLenght];
                for (int j = 0; j < WordLenght; j++)
                {
                    word[j] = list[i * WordLenght + j];
                }
                words.Add(word);
            }
            return words;
        }

        /// <summary>Converts text into binary expresion.</summary>
        /// <param name="text">Text to convert to bool list</param>
        /// <returns>Binary expresion of given text</returns>
        public static List<bool> ConvertToBooleans(string text)
        {
            List<bool> numbers = new List<bool>();
            for (int i = 0; i < text.Length; i++)
            {
                numbers = numbers.Concat(ToBinary(text[i], charSize)).ToList();
            }

            return numbers;
        }

        /// <summary>Converts bytes into binary expresion.</summary>
        /// <param name="bytes">Bytes to convert to bool list</param>
        /// <returns>Binary expresion of given bytes</returns>
        public static List<bool> ConvertToBooleans(byte[] bytes)
        {
            List<bool> numbers = new List<bool>();
            for (int i = 0; i < bytes.Length; i++)
            {
                numbers = numbers.Concat(ToBinary(bytes[i], byteSize)).ToList();
            }

            return numbers;
        }

        /// <summary>Encodes given text</summary>
        /// <param name="text">Text to encode</param>
        /// <param name="addedCount">Number of added zeros will be saved in this variable.</param>
        /// <returns>List of encoded words</returns>
        public List<bool[]> Encode(string text, out int addedCount)
        {
            List<bool[]> words = SplitToWords(ConvertToBooleans(text), out addedCount);
            List<bool[]> codes = new List<bool[]>();
            foreach (bool[] word in words)
            {
                codes.Add(Encode(word));
            }
            return codes;
        }

        /// <summary>Encodes given bytes</summary>
        /// <param name="bytes">Bytes to encode</param>
        /// <param name="addedCount">Number of added zeros will be saved in this variable.</param>
        /// <returns>List of encoded words</returns>
        public List<bool[]> Encode(byte[] bytes, out int addedCount)
        {
            List<bool[]> words = SplitToWords(ConvertToBooleans(bytes), out addedCount);
            List<bool[]> codes = new List<bool[]>();
            foreach (bool[] word in words)
            {
                codes.Add(Encode(word));
            }
            return codes;
        }
        
        /// <summary>Converts binary expresion to text</summary>
        /// <param name="binaryText">Text to convert</param>
        /// <returns>Text</returns>
        public static string ConvertToText(List<bool> binaryText)
        {
            // For binary to decimal conversion
            // 2 ^ 16, 2 ^ 15 ... 2 ^ 1, 2 ^ 0
            int[] values = new int[charSize];
            values[charSize - 1] = 1;
            for (int i = charSize - 2; i >= 0; i--)
            {
                values[i] = values[i + 1] * 2;
            }

            StringBuilder text = new StringBuilder();

            // Convert binary code to text
            int charN = 0;
            for (int i = 0; i < binaryText.Count; i += charSize)
            {
                // Convert binary to decimal
                int character = 0;
                for (int j = 0; j < charSize; j++)
                {
                    if (binaryText[charN * charSize + j])
                    {
                        int value = values[j];
                        character += value;
                    }
                }

                text.Append((char)character);
                charN++;
            }
            return text.ToString();
        }

        /// <summary>Decodes to text</summary>
        /// <param name="codes">Encoded text</param>
        /// <param name="addedCount">Number of added zeros</param>
        /// <returns>Text</returns>
        public string DecodeText(List<bool[]> codes, int addedCount)
        {
            // Decode each code line and put it together
            List<bool> binaryText = new List<bool>();
            foreach (bool[] code in codes)
            {
                List<bool> decoded = Decode(code);
                binaryText = binaryText.Concat(decoded).ToList();
            }

            // Remove added zeros
            for (int i = 0; i < addedCount; i++)
            {
                binaryText.RemoveAt(binaryText.Count - 1);
            }

            return ConvertToText(binaryText);
        }

        /// <summary>Converts binary expresion to bytes</summary>
        /// <param name="binary">Bytes to convert</param>
        /// <returns>Bytes</returns>
        public static byte[] ConvertToByteArray(List<bool> binary)
        {
            // For binary to decimal conversion
            // 2 ^ 8, 2 ^ 7 ... 2 ^ 1, 2 ^ 0
            int[] values = new int[byteSize];
            values[byteSize - 1] = 1;
            for (int i = byteSize - 2; i >= 0; i--)
            {
                values[i] = values[i + 1] * 2;
            }

            List<byte> text = new List<byte>();

            // Convert binary code to text
            int charN = 0;
            for (int i = 0; i < binary.Count; i += byteSize)
            {
                // Convert binary to decimal
                int character = 0;
                for (int j = 0; j < byteSize; j++)
                {
                    if (binary[charN * byteSize + j])
                    {
                        int value = values[j];
                        character += value;
                    }
                }

                text.Add((byte)character);
                charN++;
            }

            return text.ToArray();

        }

        /// <summary>Decodes to bytes</summary>
        /// <param name="codes">Encoded bytes</param>
        /// <param name="addedCount">Number of added zeros</param>
        /// <returns>Bytes</returns>
        public byte[] DecodeByteArray(List<bool[]> codes, int addedCount)
        {
            // Decode each code line and put it together
            List<bool> binary = new List<bool>();
            foreach (bool[] code in codes)
            {
                List<bool> decoded = Decode(code);
                binary = binary.Concat(decoded).ToList();
            }

            // Remove added zeros
            for (int i = 0; i < addedCount; i++)
            {
                binary.RemoveAt(binary.Count - 1);
            }

            return ConvertToByteArray(binary);
        }
        
        /// <summary>Multiplies 2 vectors</summary>
        /// <param name="vector1">Vector to multiply</param>
        /// <param name="vector2">Vector to multiply</param>
        /// <returns>Multiplication result</returns>
        private bool Multiply(bool[] vector1, bool[] vector2)
        {
            int matches = 0;
            for (int i = 0; i < vector1.Length; i++)
                if (vector1[i] && vector2[i])
                    matches++;
            return matches % 2 == 1;
        }

        /// <summary>Finds the final vote.</summary>
        /// <param name="votes">List of votes</param>
        /// <returns>Final vote</returns>
        /// <remarks>Returns 1 if thereare equal numbers of ones and zeros.</remarks>
        private bool GetVote(List<bool> votes)
        {
            int ones = 0;
            int zeros = 0;
            foreach(bool vote in votes)
            {
                if (vote)
                    ones++;
                else
                    zeros++;
            }
            return ones >= zeros;
        }

        /// <summary>Removes elements from original array that are in array to substract.</summary>
        /// <param name="original">Array to substract from</param>
        /// <param name="toSubstract">Array to substract</param>
        /// <returns>Original array without elements from second array</returns>
        private int[] substractArray(int[] original, int[] toSubstract)
        {
            List<int> list1 = original.ToList();
            foreach(int elem1 in toSubstract)
            {
                for (int i = 0; i < original.Length; i++)
                {
                    if (original[i] == elem1)
                    {
                        //int index = list1.IndexOf(elem1);
                        list1.Remove(elem1);
                    }
                }
            }
            return list1.ToArray();
        }

        /// <summary>Calculates decoding vectors.</summary>
        /// <param name="positions">Positions of elemnts in dictionary words to check</param>
        /// <returns>Decoding vectors</returns>
        private List<bool[]> GetDecodingVectors(int[] positions)
        {
            List<bool[]> vectors = new List<bool[]>();
            // Get all possible words of given size
            bool[][] words = GetAllWords(positions.Length);
            foreach (bool[] word in words)
            {
                bool[] vector = new bool[CodeLenght];
                for (int j = 0; j < dictionary.Length; j++)
                {
                    if (CheckWord(dictionary[j], word, positions))
                    {
                        vector[j] = true;
                    }
                }
                vectors.Add(vector);
            }

            return vectors;
        }
        
        /// <summary>Checks if values of word2 are in given positions of word1.</summary>
        /// <param name="word1">Word to check</param>
        /// <param name="word2">Word to compare to</param>
        /// <param name="positions">Positions in the first word to check</param>
        /// <returns>Do values in given positions match given values</returns>
        private bool CheckWord(bool[] word1, bool[] word2, int[] positions)
        {
            if (positions.Length != word2.Length)
                throw new ArgumentException();

            for (int i = 0; i < word2.Length; i++)
            {
                int position = positions[i];
                bool element = word2[i];
                if (word1[position] != element)
                    return false;
            }

            return true;
        }

        /// <summary>Calculates new code to continue decoding.</summary>
        /// <param name="current">Current code</param>
        /// <param name="first">First row number of matrix rows that were previously used for decoding</param>
        /// <param name="decoded">Part of vector that was previously decoded.</param>
        /// <returns>New code</returns>
        private bool[] GetNewCode(bool[] current, int first, bool[] decoded)
        {
            // Multiply part of the matrix and part of decoded vector:
            bool[] toAdd = new bool[CodeLenght];
            for (int i = 0; i< CodeLenght; i++)
            {
                bool[] toMultiply = new bool[decoded.Length];
                for (int j = 0; j < decoded.Length; j++)
                {
                    toMultiply[j] = Matrix[first + j][i];
                }
                // Put result to a new voctor:
                toAdd[i] = Multiply(decoded, toMultiply);
            }

            bool[] newCode = new bool[CodeLenght];

            // Add current and new vector:
            for (int i = 0; i < CodeLenght; i++)
            {
                newCode[i] = (current[i] && !toAdd[i]) || (!current[i] && toAdd[i]);
            }

            return newCode;
        }

        /// <summary>Gets all words of given size</summary>
        /// <param name="length">Word length</param>
        /// <returns>Words array</returns>
        /// <example>length = 2 : 00, 01, 10, 11</example>
        private bool[][] GetAllWords(int length)
        {
            int wordCount = 1;
            for (int i = 0; i < length; i++)
                wordCount *= 2;

            bool[][] words = new bool[wordCount][];
            for (int i = 0; i < wordCount; i++)
            {
                words[i] = ToBinary(i, length).ToArray();
            }
            return words;
        }

        /// <summary>Decodes encoded vector.</summary>
        /// <param name="code">Encoded vector</param>
        /// <returns>Decoded vector</returns>
        public List<bool> Decode(bool[] code)
        {
            List<bool> word = new List<bool>();

            // Array of first level row indexes in the matrix:
            int[] indexArray = new int[m];
            for (int i = 1; i <= m; i++)
            {
                indexArray[i - 1] = i;
            }

            // Current position in decoded vector
            int current = WordLenght - 1;

            // Current matrix row level
            int r1 = r;

            while (current >= 0)
            {
                // Count of rows to use
                int count = GetCombinationsCount(m, r1);

                // Index of last row
                int min = current - count;
                
                // Itterating one level rows.
                // For example:
                // m = 3, r = 2

                // r1 = 2
                // 1  1  1  1  1  1  1  1
                // 1  1  1  1  0  0  0  0
                // 1  1  0  0  1  1  0  0
                // 1  0  1  0  1  0  1  0
                // 1  1  0  0  0  0  0  0 <-
                // 1  0  1  0  0  0  0  0 <-
                // 1  0  0  0  1  0  0  0 <-

                // r1 = 1
                // 1  1  1  1  1  1  1  1
                // 1  1  1  1  0  0  0  0 <-
                // 1  1  0  0  1  1  0  0 <-
                // 1  0  1  0  1  0  1  0 <-
                // 1  1  0  0  0  0  0  0
                // 1  0  1  0  0  0  0  0
                // 1  0  0  0  1  0  0  0
                for (int i = current; i > min; i--)
                {
                    if (r1 == 0)
                    {
                        word.Insert(0, GetVote(code.ToList()));
                        return word;
                    }

                    List<bool> finalVotes = new List<bool>();
                    List<int[]> combinations = GetCombinations(indexes, r1);
                    current--;
                    
                    for (int j = 1; j <= count; j++)
                    {
                        int[] positions = substractArray(indexArray, combinations[j - 1]);
                        for (int k = 0; k < positions.Length; k++)
                        {
                            positions[k]--;
                        }

                        List<bool[]> decodingVectors = GetDecodingVectors(positions);
                        List<bool> votes = new List<bool>();
                        foreach (bool[] vector in decodingVectors)
                        {
                            votes.Add(Multiply(code, vector));
                        }
                        finalVotes.Add(GetVote(votes));
                    }
                    for (int j = finalVotes.Count - 1; j >= 0; j--)
                        word.Insert(0, finalVotes[j]);
                    
                    r1--;
                    code = GetNewCode(code, min + 1, finalVotes.ToArray());
                    count = GetCombinationsCount(m, r1);
                    min -= count;
                }
            }
            return null;
        }
    }
}
