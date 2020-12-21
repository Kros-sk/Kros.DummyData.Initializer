using MMLib.RapidPrototyping.Generators;
using MMLib.RapidPrototyping.Models;
using Scriban.Runtime;
using System;
using System.Text;

namespace Kros.DummyData.Initializer
{
    /// <summary>
    /// Scriban template functions.
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
    public class TemplateFunctions: ScriptObject
    {
        private static readonly Random _randomInt = new Random();
        private static readonly Random _randomDouble = new Random();
        private static readonly LoremIpsumGenerator _loremIpsum = new LoremIpsumGenerator();
        private static readonly PersonGenerator _personGenerator = new PersonGenerator();

        /// <summary>
        /// Randoms the int.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public static string RandomInt(int min, int max)
            => _randomInt.Next(min, max).ToString();

        /// <summary>
        /// Randoms the double.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public static string RandomDouble(double min, double max)
            => ((_randomDouble.NextDouble() * (min - max)) + min).ToString();

        /// <summary>
        /// Lorems the ipsum.
        /// </summary>
        /// <param name="length">The length.</param>
        public static string LoremIpsum(int length)
        {
            var sb = new StringBuilder(length);

            while(sb.Length < length)
            {
                var text = _loremIpsum.Next(1, 100).Replace(Environment.NewLine, "");
                var l = length - sb.Length;
                if (l> text.Length)
                {
                    l = text.Length;
                }
                sb.Append(text.Substring(0, l));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Randoms the first name.
        /// </summary>
        public static string RandomFirstName()
            => _personGenerator.Next().FirstName;

        /// <summary>
        /// Randoms the last name.
        /// </summary>
        public static string RandomLastName()
            => _personGenerator.Next().LastName;

        /// <summary>
        /// Randoms the email.
        /// </summary>
        public static string RandomEmail()
            => _personGenerator.Next().Mail;

        /// <summary>
        /// Randoms the person.
        /// </summary>
        /// <param name="separator">The separator.</param>
        public static string RandomPerson(string separator)
        {
            IPerson person = _personGenerator.Next();

            return $"{person.FirstName}{separator}{person.LastName}";
        }

        /// <summary>
        /// Randoms the person.
        /// </summary>
        /// <param name="separator">The separator.</param>
        public static IPerson RandomPerson()
            => _personGenerator.Next();
    }
}
