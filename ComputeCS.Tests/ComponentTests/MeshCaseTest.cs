using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ComputeCS.types;
using ComputeCS;

namespace ComputeCS.Tests.ComponentTests
{
    [TestFixture]
    public class TestMesh
    {

        /* Component core dictionary input - this is always a JSON string
        containing core parameters from upstream components that may be 
        needed in downstream components.  
        Downstream components will get or default or error against these 
        parameters.
        */

        /* Component inputs*/

        // Inputs for BlockMesh
        private string type;

        private double cellSize;

        private string domain;
        // Inputs for SnappyHexMesh
        private Dictionary<string, object> defaultSurfaces;


        [SetUp]
        public void SetUp()
        {

            // Input parameters (these will be input into the component)
            type = "simpleBox";
            cellSize = 0.25;

            domain = @"{
            'mesh': {
                'base_mesh': {
                    'type': 'simpleBox',
                    'cell_size': 2.0,
                    'bounding_box': {
                        'min': [
                        -35.0,
                        -35.0,
                        0.0
                            ],
                        'max': [
                        35.0,
                        35.0,
                        24.0
                            ]
                    },
                    'parameters': {
                        'square': 'True',
                        'z0': 'True'
                    }
                },
                'snappy_hex_mesh': {
                    'overrides': {
                        'castellatedMeshControls': {
                            'locationInMesh': [
                            14.959544095599346,
                            14.959544095599346,
                            17.128986547062631
                                ]
                        }
                    },
                    'default_surface': null,
                    'surfaces': {
                        '_48ec977c-a254-4265-b1f6-887f22a5789a': {
                            'level': {
                                'min': '2',
                                'max': '2'
                            }
                        },
                        'ef915c80-37ff-444d-b8c1-6da92bbb3c42': {
                            'level': {
                                'min': '2',
                                'max': '2'
                            }
                        },
                        'ae36b695-6e4f-4731-b291-169babab88e7': {
                            'level': {
                                'min': '2',
                                'max': '2'
                            }
                        },
                        '_5c9f4ce8-d706-4f3d-9c08-4fd9e3f10fc1': {
                            'level': {
                                'min': '2',
                                'max': '2'
                            }
                        },
                        'e7cb3704-5c14-43f3-85f1-6fe455e183d9': {
                            'level': {
                                'min': '2',
                                'max': '2'
                            }
                        }
                    }
                }
            }";
            defaultSurfaces = new Dictionary<string, object>
            {
                {"name", "Plane"},
                {"level", new List<int>{3, 3}}
            };
        }

        [Test]
        public void TestMeshCase()
        {
            var outputs = ComputeCS.Components.Mesh.Setup(
                "",
                domain,
                defaultSurfaces,
                null
            );
            
            Console.WriteLine($"Got Output: {outputs["out"]}");
            // Components should always output json string "out" - this is the core dictoinary
            Assert.IsTrue(outputs.ContainsKey("out"));
        }
    }
}