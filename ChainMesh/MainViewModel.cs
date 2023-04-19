using CommunityToolkit.Mvvm.ComponentModel;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainMesh {
    public partial class MainViewModel : ObservableObject {

        private int coordinateCount = 1;
        private float size = 1;
        private int theta = 64;



        [ObservableProperty]
        MeshGeometry3D geometry;

        [ObservableProperty]
        ObservableCollection<string> shapeTypes;

        [ObservableProperty]
        string selectedShapeType;

        public EffectsManager Effectsmanager { get; set; }
        public OrthographicCamera Camera { get; set; }

        public PBRMaterial MetalMaterial { get; set; }


        public MainViewModel() {
            Effectsmanager = new DefaultEffectsManager();
            string folder = Environment.CurrentDirectory;

            //Camera
            Camera = new OrthographicCamera {
                Position = new System.Windows.Media.Media3D.Point3D(0, 0, 0),
                LookDirection = new System.Windows.Media.Media3D.Vector3D(-2, 1, -1.75),
                UpDirection = new System.Windows.Media.Media3D.Vector3D(-0.5, 0.4, 0.75),
                FarPlaneDistance = 2000,
                NearPlaneDistance = -2000,
                Width = 10

            };


            MetalMaterial = new PBRMaterial() {
                AlbedoColor = Color.White,
                AlbedoMap = LoadFileToMemory(folder + @"\Metal\Metal026_2K_Color.jpg"),
                DisplacementMap = LoadFileToMemory(folder + @"\Metal\Metal026_2K_Displacement.jpg"),
                NormalMap = LoadFileToMemory(folder + @"\Metal\Metal026_2K_NormalDX.jpg"),
                EnableAutoTangent = true,
                RoughnessFactor = 0.5,
                // MetallicFactor = 0,
                // ReflectanceFactor = 0,
                RenderDisplacementMap = true,
                //DisplacementMapScaleMask = new Vector4(displacementScale, displacementScale, displacementScale, displacementScale),
                RenderAlbedoMap = true,
                RenderNormalMap = true,
                Name = "Metal"

            };



            shapeTypes = new ObservableCollection<string>(new string[] { "Box", "Sphere", "Plane", "Cylinder", "Chain" });
            selectedShapeType = "Chain";
            CreateGeometry();

        }

        public MemoryStream LoadFileToMemory(string filePath) {

            using (var file = new FileStream(filePath, FileMode.Open)) {
                var memory = new MemoryStream();
                file.CopyTo(memory);
                return memory;
            }

        }


        private MeshGeometry3D CreateCylinder() {
            var meshbuilder = new MeshBuilder();
            meshbuilder.AddCylinder(new Vector3(0, 0, 0), new Vector3(0, 0, 0.1f), 0.05f);
            MeshGeometry3D meshGeometry3D = meshbuilder.ToMeshGeometry3D();
            for (int i = 0; i < meshGeometry3D.TextureCoordinates.Count; ++i) {
                meshGeometry3D.TextureCoordinates[i] *= coordinateCount;
            }
            meshGeometry3D.UpdateTextureCoordinates();
            return meshGeometry3D;

        }
        private MeshGeometry3D CreatePlane() {
            var meshbuilder = new MeshBuilder();
            var quad = new List<Vector3>() {
                Vector3.Zero,
                new Vector3(size,0,0),
                new Vector3(size,size,0),
                new Vector3(0,size,0) };


            meshbuilder.AddQuad(quad[0], quad[1], quad[2], quad[3]);
            MeshGeometry3D meshGeometry3D = meshbuilder.ToMeshGeometry3D();
            for (int i = 0; i < meshGeometry3D.TextureCoordinates.Count; ++i) {
                meshGeometry3D.TextureCoordinates[i] *= coordinateCount;
            }
            meshGeometry3D.UpdateTextureCoordinates();

            return meshGeometry3D;

        }
        private MeshGeometry3D CreateSphere() {
            var meshbuilder = new MeshBuilder();
            meshbuilder.AddSphere(new Vector3(0, 0, 0), size, theta, theta);
            MeshGeometry3D meshGeometry3D = meshbuilder.ToMeshGeometry3D();
            for (int i = 0; i < meshGeometry3D.TextureCoordinates.Count; ++i) {
                meshGeometry3D.TextureCoordinates[i] *= coordinateCount;
            }
            meshGeometry3D.UpdateTextureCoordinates();
            return meshGeometry3D;

        }

        private MeshGeometry3D CreateBox() {
            var meshbuilder = new MeshBuilder();
            meshbuilder.AddBox(new Vector3(0, 0, 0), size, size, size);
            MeshGeometry3D meshGeometry3D = meshbuilder.ToMeshGeometry3D();
            for (int i = 0; i < meshGeometry3D.TextureCoordinates.Count; ++i) {
                meshGeometry3D.TextureCoordinates[i] *= coordinateCount;
            }
            meshGeometry3D.UpdateTextureCoordinates();
            return meshGeometry3D;

        }

        private MeshGeometry3D CreateChain() {

            var chainmesh = CreateMeshChain(Vector3.Zero, new Vector3(0, 0, 10), 64);

            MeshGeometry3D meshGeometry3D = chainmesh.ToMeshGeometry3D();
            for (int i = 0; i < meshGeometry3D.TextureCoordinates.Count; ++i) {
                meshGeometry3D.TextureCoordinates[i] *= coordinateCount;
            }

            meshGeometry3D.CalculateNormals();
            meshGeometry3D.UpdateVertices();
            meshGeometry3D.UpdateColors();
            meshGeometry3D.UpdateTextureCoordinates();

            return meshGeometry3D;

        }

        private void CreateGeometry() {
            switch (selectedShapeType) {
                case "Sphere":
                    Geometry = CreateSphere();
                    break;
                case "Box":
                    Geometry = CreateBox();
                    break;
                case "Plane":
                    Geometry = CreatePlane();
                    break;
                case "Cylinder":
                    Geometry = CreateCylinder();
                    break;
                case "Chain":
                    Geometry = CreateChain();
                    break;
            }
        }

        partial void OnSelectedShapeTypeChanged(string value) {
            CreateGeometry();
        }




        public MeshBuilder CreateMeshChain(Vector3 start, Vector3 end, int theta) {

            // Chain data
            float width = 0.1f;
            float length = 0.1f;
            float diameter = 0.03f;
            int numOfCopies = (int)(Vector3.Distance(start, end) / length);



            MeshBuilder meshBuilder = new MeshBuilder();


            float tubeRadius = (width - diameter) / 2;
            float tubeStraight = length + diameter - 2 * tubeRadius;
            float trans = 0f;
            float translate = tubeStraight + (tubeRadius * 2) - diameter;
            int segments = 10;
            float interval = 180 / segments;
            Vector3 yVector = Vector3.UnitY;

            Matrix rotationMatrix = RotationMatrix(Vector3.Zero, yVector, start, end);


            //The for loop is drawing the chainlink 
            for (int j = 0; j < numOfCopies; j++) {


                List<Vector3> single_chain_link = new List<Vector3>();

                for (float i = 360; i >= 0; i -= interval) {

                    // Debug.WriteLine($"i= {i}");
                    float yoffset = 0;
                    if (i < 180)
                        yoffset = tubeStraight;


                    float a = i * MathF.PI / 180;
                    float x = tubeRadius * MathF.Cos(a);
                    float y = tubeRadius * MathF.Sin(a) + yoffset + trans + (tubeRadius - (diameter / 2));
                    Vector3 vec = new Vector3(x, y, 0);


                    //Rotates every second chainlink
                    if (j % 2 == 1)
                        vec = new Vector3(0, y, x);


                    var newVec = Vector3.TransformCoordinate(vec, rotationMatrix);
                    vec += start;
                    single_chain_link.Add(vec);
                }
                meshBuilder.AddTube(single_chain_link, diameter, theta, true);




                trans += translate;
            }
            return meshBuilder;
        }


        public Matrix RotationMatrix(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2) {
            Vector3 v1 = a2 - a1;
            Vector3 v2 = b2 - b1;

            float angle = (float)Math.Acos(Vector3.Dot(v1, v2) / (v1.Length() * v2.Length()));
            Vector3 axis = Vector3.Cross(v1, v2);
            axis = Vector3.Normalize(axis);
            return Matrix.RotationAxis(axis, angle);



        }

    }
}
