using NUnit.Framework;
using System;
using ComputeCS.types;


namespace ComputeCS.Tests
{
    [TestFixture]
    public class TestSerialize
    {           
        [Test]
        public void TestSerializeInputs()
        {
            // Load the user class (which loads the secrets on model instantiation)
            Inputs inputs = new Inputs();
            inputs.Url = "TestUrl";

            string json = inputs.ToJson();

            Console.WriteLine($"ToJson: {json}");

            Inputs newInputs = new Inputs().FromJson(json);

            Console.WriteLine($"FromJson[Url]: {newInputs.Url}");
 
            Assert.AreEqual(newInputs.Url, "TestUrl");
        }

        [Test]
        public void TestSerializeNestedInputs()
        {
            // Load the user class (which loads the secrets on model instantiation)
            Inputs inputs = new Inputs {
                 Url = "TestUrl",
                 CFDSolution = new CFDSolution {
                    Solver = "simpleFoam"
                }
            };
            
            string json = inputs.ToJson();

            Console.WriteLine($"ToJson: {json}");

            Inputs newInputs = new Inputs().FromJson(json);

            Console.WriteLine($"FromJson[Url]: {newInputs.Url}");
            Console.WriteLine($"FromJson[CFDSolution.Solver]: {newInputs.CFDSolution.Solver}");
 
            Assert.AreEqual(newInputs.Url, "TestUrl");
            Assert.AreEqual(newInputs.CFDSolution.Solver, "simpleFoam");
        }
    }
}