using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using Random = System.Random;

struct Cringxel
{
    public byte x, y, z, color;

    public Cringxel(BinaryReader stream)
    {
        x = stream.ReadByte();
        y = stream.ReadByte();
        z = stream.ReadByte();
        color = stream.ReadByte();
    }
}

public class Pirate : MonoBehaviour
{
    
    [SerializeField] private Vector3Int dims = new Vector3Int(10, 10, 6);


    // Start is called before the first frame update
    void Start()
    {
        Random random = new Random();
        var xdoc = new XmlDocument();
        xdoc.Load("Assets/samples.xml");

        int counter = 1;
        foreach (XmlNode xnode in xdoc.FirstChild.ChildNodes)
        {
            if (xnode.Name == "#comment") continue;

            string name = xnode.Get<string>("name");
            Debug.Log($"{name}");

            Model model = new Model(name, "subset", dims.x, dims.y, dims.z, true, "ground", 3);

            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 100; k++)
                {
                    int seed = random.Next();
                    bool finished = model.Run(seed);
                    if (finished)
                    {
                        Debug.Log("DONE");

                        string output = model.TextOutput();
                        System.IO.File.WriteAllText($"{counter} {name} {i}.txt", output);
                        Debug.Log(output);
                        //var scene = model.VoxelOutput();

                        break;
                    }
                    else Debug.Log("CONTRADICTION");
                }
            }

            counter++;
        }
    }
}

class Model
{
    bool[][][][] wave;
    bool[][][] changes;
    int[][][] observed;
    double[] stationary;

    protected int FMX, FMY, FMZ, T, ground;
    protected bool periodic;

    double[] logProb;
    double logT;

    bool[][][] propagator;

    //List<Color[]> tiles;
    List<string> tilenames;
    List<Cringxel[]> voxeltiles;
    int voxelsize;

    public Model(string name, string subsetName, int FMX, int FMY, int FMZ, bool periodic, string groundName, int voxelsize)
    {
        this.FMX = FMX;
        this.FMY = FMY;
        this.FMZ = FMZ;
        this.voxelsize = voxelsize;
        this.periodic = periodic;
        ground = -1;

        var xdoc = new XmlDocument();
        xdoc.Load($"Assets/{name}/data.xml");
        XmlNode xnode = xdoc.FirstChild;
        xnode = xnode.FirstChild;

        List<string> subset = null;
        if (subsetName != default(string))
        {
            subset = new List<string>();
            foreach (XmlNode xsubset in xnode.NextSibling.NextSibling.ChildNodes)
                if (xsubset.NodeType != XmlNodeType.Comment && xsubset.Get<string>("name") == subsetName)
                    foreach (XmlNode stile in xsubset.ChildNodes) subset.Add(stile.Get<string>("name"));
        }
        /*
        Func<Func<int, int, Color>, Color[]> tile = f =>
        {
            Color[] result = new Color[pixelsize * pixelsize];
            for (int y = 0; y < pixelsize; y++) for (int x = 0; x < pixelsize; x++) result[x + y * pixelsize] = f(x, y);
            return result;
        };*/

        //Func<Color[], Color[]> rotate = array => tile((x, y) => array[pixelsize - 1 - y + x * pixelsize]);
        Func<Cringxel[], Cringxel[]> rotateVoxels = array => array.Select(v => new Cringxel { x = (byte)(voxelsize - 1 - v.y), y = v.x, z = v.z, color = v.color }).ToArray();

        //tiles = new List<Color[]>();
        tilenames = new List<string>();
        voxeltiles = new List<Cringxel[]>();
        var tempStationary = new List<double>();

        List<int[]> action = new List<int[]>();
        Dictionary<string, int> firstOccurrence = new Dictionary<string, int>();

        foreach (XmlNode xtile in xnode.ChildNodes)
        {
            string tilename = xtile.Get<string>("name");
            if (subset != null && !subset.Contains(tilename)) continue;

            Func<int, int> a, b;
            int cardinality;

            char sym = xtile.Get("symmetry", 'X');
            if (sym == 'L')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i + 1 : i - 1;
            }
            else if (sym == 'T')
            {
                cardinality = 4;
                a = i => (i + 1) % 4;
                b = i => i % 2 == 0 ? i : 4 - i;
            }
            else if (sym == 'I')
            {
                cardinality = 2;
                a = i => 1 - i;
                b = i => i;
            }
            else if (sym == '\\')
            {
                cardinality = 2;
                a = i => 1 - i;
                b = i => 1 - i;
            }
            else
            {
                cardinality = 1;
                a = i => i;
                b = i => i;
            }

            T = action.Count;
            firstOccurrence.Add(tilename, T);
            if (tilename == groundName) ground = T;

            int[][] map = new int[cardinality][];
            for (int t = 0; t < cardinality; t++)
            {
                map[t] = new int[8];

                map[t][0] = t;
                map[t][1] = a(t);
                map[t][2] = a(a(t));
                map[t][3] = a(a(a(t)));
                map[t][4] = b(t);
                map[t][5] = b(a(t));
                map[t][6] = b(a(a(t)));
                map[t][7] = b(a(a(a(t))));

                for (int s = 0; s < 8; s++) map[t][s] += T;

                action.Add(map[t]);
            }

            // Read image
            /*Bitmap bitmap = new Bitmap($"{name}/{tilename}.png");*/
            Cringxel[] voxeltile = Stuff.ReadVox($"Assets/{name}/{tilename}.vox");

            //tiles.Add(tile((x, y) => bitmap.GetPixel(x, y)));
            tilenames.Add($"{tilename} 0");
            voxeltiles.Add(voxeltile);

            for (int t = 1; t < cardinality; t++)
            {
                //tiles.Add(rotate(tiles[T + t - 1]));
                tilenames.Add($"{tilename} {t}");
                voxeltiles.Add(rotateVoxels(voxeltiles[T + t - 1]));
            }

            for (int t = 0; t < cardinality; t++) tempStationary.Add(xtile.Get("weight", 1.0f));
        }

        T = action.Count;
        stationary = tempStationary.ToArray();

        propagator = new bool[6][][];
        for (int d = 0; d < 6; d++)
        {
            propagator[d] = new bool[T][];
            for (int t = 0; t < T; t++) propagator[d][t] = new bool[T];
        }

        wave = new bool[FMX][][][];
        changes = new bool[FMX][][];
        observed = new int[FMX][][];
        for (int x = 0; x < FMX; x++)
        {
            wave[x] = new bool[FMY][][];
            changes[x] = new bool[FMY][];
            observed[x] = new int[FMY][];
            for (int y = 0; y < FMY; y++)
            {
                wave[x][y] = new bool[FMZ][];
                changes[x][y] = new bool[FMZ];
                observed[x][y] = new int[FMZ];
                for (int z = 0; z < FMZ; z++)
                {
                    wave[x][y][z] = new bool[T];
                    observed[x][y][z] = -1;
                }
            }
        }

        foreach (XmlNode xneighbor in xnode.NextSibling.ChildNodes)
        {
            string[] left = xneighbor.Get<string>("left").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] right = xneighbor.Get<string>("right").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (subset != null && (!subset.Contains(left[0]) || !subset.Contains(right[0]))) continue;

            int L = action[firstOccurrence[left[0]]][left.Length == 1 ? 0 : int.Parse(left[1])], D = action[L][1];
            int R = action[firstOccurrence[right[0]]][right.Length == 1 ? 0 : int.Parse(right[1])], U = action[R][1];

            if (xneighbor.Name == "horizontal")
            {
                propagator[0][R][L] = true;
                propagator[0][action[R][6]][action[L][6]] = true;
                propagator[0][action[L][4]][action[R][4]] = true;
                propagator[0][action[L][2]][action[R][2]] = true;

                propagator[1][U][D] = true;
                propagator[1][action[D][6]][action[U][6]] = true;
                propagator[1][action[U][4]][action[D][4]] = true;
                propagator[1][action[D][2]][action[U][2]] = true;
            }
            else for (int g = 0; g < 8; g++) propagator[4][action[L][g]][action[R][g]] = true;
        }

        for (int t2 = 0; t2 < T; t2++) for (int t1 = 0; t1 < T; t1++)
            {
                propagator[2][t2][t1] = propagator[0][t1][t2];
                propagator[3][t2][t1] = propagator[1][t1][t2];
                propagator[5][t2][t1] = propagator[4][t1][t2];
            }
    }

    bool? Observe()
    {
        double min = 1E+3, sum, mainSum, logSum, noise, entropy;
        int argminx = -1, argminy = -1, argminz = -1, amount;
        bool[] w;
        Random random = new Random();

        for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++) for (int z = 0; z < FMZ; z++)
                {
                    w = wave[x][y][z];
                    amount = 0;
                    sum = 0;

                    for (int t = 0; t < T; t++) if (w[t])
                        {
                            amount += 1;
                            sum += stationary[t];
                        }

                    if (sum == 0) return false;

                    noise = 1E-6 * random.NextDouble();

                    if (amount == 1) entropy = 0;
                    else if (amount == T) entropy = logT;
                    else
                    {
                        mainSum = 0;
                        logSum = Math.Log(sum);
                        for (int t = 0; t < T; t++) if (w[t]) mainSum += stationary[t] * logProb[t];
                        entropy = logSum - mainSum / sum;
                    }

                    if (entropy > 0 && entropy + noise < min)
                    {
                        min = entropy + noise;
                        argminx = x;
                        argminy = y;
                        argminz = z;
                    }
                }

        if (argminx == -1 && argminy == -1 && argminz == -1)
        {
            for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++) for (int z = 0; z < FMZ; z++) for (int t = 0; t < T; t++) if (wave[x][y][z][t])
                            {
                                observed[x][y][z] = t;
                                break;
                            }

            return true;
        }

        double[] distribution = new double[T];
        for (int t = 0; t < T; t++) distribution[t] = wave[argminx][argminy][argminz][t] ? stationary[t] : 0;
        int r = distribution.Random(random.NextDouble());
        for (int t = 0; t < T; t++) wave[argminx][argminy][argminz][t] = t == r;
        changes[argminx][argminy][argminz] = true;

        return null;
    }

    bool Propagate()
    {
        bool change = false, b;
        for (int x2 = 0; x2 < FMX; x2++) for (int y2 = 0; y2 < FMY; y2++) for (int z2 = 0; z2 < FMZ; z2++) for (int d = 0; d < 6; d++)
                    {
                        int x1 = x2, y1 = y2, z1 = z2;
                        if (d == 0)
                        {
                            if (x2 == 0)
                            {
                                if (!periodic) continue;
                                else x1 = FMX - 1;
                            }
                            else x1 = x2 - 1;
                        }
                        else if (d == 1)
                        {
                            if (y2 == FMY - 1)
                            {
                                if (!periodic) continue;
                                else y1 = 0;
                            }
                            else y1 = y2 + 1;
                        }
                        else if (d == 2)
                        {
                            if (x2 == FMX - 1)
                            {
                                if (!periodic) continue;
                                else x1 = 0;
                            }
                            else x1 = x2 + 1;
                        }
                        else if (d == 3)
                        {
                            if (y2 == 0)
                            {
                                if (!periodic) continue;
                                else y1 = FMY - 1;
                            }
                            else y1 = y2 - 1;
                        }
                        else if (d == 4)
                        {
                            if (z2 == FMZ - 1)
                            {
                                if (!periodic) continue;
                                else z1 = 0;
                            }
                            else z1 = z2 + 1;
                        }
                        else
                        {
                            if (z2 == 0)
                            {
                                if (!periodic) continue;
                                else z1 = FMZ - 1;
                            }
                            else z1 = z2 - 1;
                        }

                        if (!changes[x1][y1][z1]) continue;

                        bool[] w1 = wave[x1][y1][z1];
                        bool[] w2 = wave[x2][y2][z2];

                        for (int t2 = 0; t2 < T; t2++) if (w2[t2])
                            {
                                bool[] prop = propagator[d][t2];
                                b = false;

                                for (int t1 = 0; t1 < T && !b; t1++) if (w1[t1]) b = prop[t1];
                                if (!b)
                                {
                                    w2[t2] = false;
                                    changes[x2][y2][z2] = true;
                                    change = true;
                                }
                            }
                    }

        return change;
    }

    public bool Run(int seed)
    {
        Random random = new Random(seed);
        logT = Math.Log(T);
        logProb = new double[T];
        for (int t = 0; t < T; t++) logProb[t] = Math.Log(stationary[t]);

        Clear();

        random = new Random(seed);

        while (true)
        {
            bool? result = Observe();
            if (result != null) return (bool)result;
            while (Propagate()) ;
        }
    }

    void Clear()
    {
        for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++) for (int z = 0; z < FMZ; z++)
                {
                    for (int t = 0; t < T; t++) wave[x][y][z][t] = true;
                    changes[x][y][z] = false;
                }

        if (ground >= 0)
        {
            for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++)
                {
                    for (int t = 0; t < T; t++) if (t != ground) wave[x][y][FMZ - 1][t] = false;
                    changes[x][y][FMZ - 1] = true;

                    for (int z = 0; z < FMZ - 1; z++)
                    {
                        wave[x][y][z][ground] = false;
                        changes[x][y][z] = true;
                    }
                }
        }
    }

      public string TextOutput()
    {
        var result = new System.Text.StringBuilder();

        for (int z = 0; z < FMZ; z++)
        {
            for (int y = 0; y < FMY; y++)
            {
                for (int x = 0; x < FMX; x++) result.Append($"{tilenames[observed[x][y][z]]}, ");
                result.Append(Environment.NewLine);
            }

            result.Append(Environment.NewLine);
        }

        return result.ToString();
    }

    public Tuple<int, int, int, Cringxel[]> VoxelOutput()
    {
        List<Cringxel> result = new List<Cringxel>();

        for (int x = 0; x < FMX; x++) for (int y = 0; y < FMY; y++) for (int z = 0; z < FMZ; z++)
                {
                    Cringxel[] voxeltile = voxeltiles[observed[x][FMY - y - 1][FMZ - z - 1]];
                    foreach (Cringxel v in voxeltile) result.Add(new Cringxel
                    {
                        x = (byte)(v.x + x * voxelsize),
                        y = (byte)(v.y + y * voxelsize),
                        z = (byte)(v.z + z * voxelsize),
                        color = v.color
                    });
                }

        return new Tuple<int, int, int, Cringxel[]>(FMX * voxelsize, FMY * voxelsize, FMZ * voxelsize, result.ToArray());
    }
}


static class Stuff
{
    public static int Random(this double[] a, double r)
    {
        double sum = a.Sum();

        if (sum == 0)
        {
            for (int j = 0; j < a.Count(); j++) a[j] = 1;
            sum = a.Sum();
        }

        for (int j = 0; j < a.Count(); j++) a[j] /= sum;

        int i = 0;
        double x = 0;

        while (i < a.Count())
        {
            x += a[i];
            if (r <= x) return i;
            i++;
        }

        return 0;
    }

    public static T Get<T>(this XmlNode node, string attribute, T defaultT = default(T))
    {
        string s = ((XmlElement)node).GetAttribute(attribute);
        var converter = TypeDescriptor.GetConverter(typeof(T));
        return s == "" ? defaultT : (T)converter.ConvertFromInvariantString(s);
    }

    public static Cringxel[] ReadVox(string filename) => ReadVox(new BinaryReader(File.Open(filename, FileMode.Open)));

    static Cringxel[] ReadVox(BinaryReader stream)
    {
        Cringxel[] voxels = null;

        string magic = new string(stream.ReadChars(4));
        int version = stream.ReadInt32();

        while (stream.BaseStream.Position < stream.BaseStream.Length)
        {
            char[] chunkId = stream.ReadChars(4);
            int chunkSize = stream.ReadInt32();
            int childChunks = stream.ReadInt32();
            string chunkName = new string(chunkId);

            if (chunkName == "SIZE")
            {
                int X = stream.ReadInt32();
                int Y = stream.ReadInt32();
                int Z = stream.ReadInt32();
                stream.ReadBytes(chunkSize - 4 * 3);
            }
            else if (chunkName == "XYZI")
            {
                int numVoxels = stream.ReadInt32();
                voxels = new Cringxel[numVoxels];
                for (int i = 0; i < voxels.Length; i++) voxels[i] = new Cringxel(stream);
            }
        }
        return voxels;
    }
}
