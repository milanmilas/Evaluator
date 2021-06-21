using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Parser
{
    public static class Evaluator
    {
        public static long Evaluate(string path)
        {
            List<(int location, Func<ICollection<long>, long>? operationFunction, List<int> operationArgsLocations)> expressions = GetExpressions(path)
                .Select(lineExpression =>  Parse(lineExpression))
                .ToList();

            var expressionsByLocation = expressions
                .ToDictionary(x => x.location, x => x);

            var evaluatedExpressionsByLocationCache = expressions
                .Where(x => x.operationFunction == null)
                .ToDictionary(x => x.location, x => (long)x.operationArgsLocations[0]);

            // NOTE: Evaluate has side effect on evaluatedExpressionsByLocationCache due to recursion implementation
            // named Tuples are used due to number of tuple elements not exceeding 3, there is slight overhead compared to classes
            return Evaluate(expressions.First(), expressionsByLocation, evaluatedExpressionsByLocationCache);

            IEnumerable<string> GetExpressions(string path)
            {
                using var fileReader = new StreamReader(path);

                string line = null;
                while ((line = fileReader.ReadLine()) != null)
                {
                    yield return line;
                }
            }

            (int location, Func<ICollection<long>, long>? operationFunction, List<int> operationArgsLocations) Parse(string lineExpression)
            {
                var locExpressionParts = lineExpression.Split(':');
                var location = int.Parse(locExpressionParts[0]);
                var expressionParts = locExpressionParts[1].Trim().Split(' ');
                var operationName = expressionParts[0];
                Func<ICollection<long>, long>? operationFunction =
                       operationName == "Add"
                       ? (args) => args.Sum()
                       : operationName == "Mult"
                           ? (args) => args.Aggregate<long, long>(1, (acc, val) => acc * val)
                           : null;

                var operationArgsLocations = expressionParts.Skip(1).Select(stringExpressionPart => int.Parse(stringExpressionPart)).ToList();

                return (location, operationFunction, operationArgsLocations);
            }

            long Evaluate(
                (int location, Func<ICollection<long>,long> operationFunction, List<int> operationArgsLocations) expression,
                Dictionary<int, (int location, Func<ICollection<long>, long> operationFunction, List<int> operationArgsLocations)> expressionsByLocation,
                Dictionary<int, long> evaluatedExpressionsByLocationCache)
            {
                var evaluatedArgs = expression
                            .operationArgsLocations
                            .Select(operationArgsLocation =>
                            {
                                return evaluatedExpressionsByLocationCache.TryGetValue(operationArgsLocation, out var expressionValue)
                                    ? expressionValue
                                    : (evaluatedExpressionsByLocationCache[operationArgsLocation] = Evaluate(expressionsByLocation[operationArgsLocation],
                                                                                                                expressionsByLocation,
                                                                                                                evaluatedExpressionsByLocationCache));
                            }).ToList();

                return expression.operationFunction(evaluatedArgs);
            }
        }
    }           

    class Program
    {
        static void Main(string[] args)
        {
            var result = Evaluator.Evaluate("input.txt");
            Console.WriteLine(result);
        }
    }
}
