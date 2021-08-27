using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.IO;


namespace SMS
{
    class Simulation
    {
        public List<Organism> Adults = new List<Organism>();
        public List<Organism> Eggs = new List<Organism>();
        public static Random random = new Random();
      
        /*-------------------- Simulation Parameters ---------------------------------*/

        public int Generations = 15;
        public int Iterations = 10;
        public float eggchance = 0.95f;
        public int PopulationCap = 1000;
        public float Mortality = 0.1f;
        public int GlobalEggsPerFemale = 50;
        public int Sample = 0;
    
      

        public bool ApplyIntervention = false;

 public int StartingNumberOfWTFemales = 150;
 public int StartingNumberOfWTMales = 150;
        public int StartIntervention = 30;
        public int EndIntervention = 30;
        public int StartInterventionsecond = 30;
        public int EndInterventionsecond = 30;
        public int White_drive_male = 0;
        public int White_drive_female = 0;
        public int Nudel_male = 100;
        public int Nudel_female = 0;

        string[] White_Track = { "White" };
        //string[] Nudel_Track = { "Nudel" };



        /*------------------------------- The Simulation ---------------------------------------------*/

        public void Simulate()
        { 
            string pathdesktop = (string)Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            pathdesktop = pathdesktop + "/model/Final_model_data";
            string pathString = System.IO.Path.Combine(pathdesktop, "zpg w female.csv");



            Console.WriteLine("Writing output to: " + pathString);
            File.Create(pathString).Dispose();

            Console.WriteLine("Simulation Starts.");

            using (var stream = File.OpenWrite(pathString))
            using (var Fwriter = new StreamWriter(stream))
            {
                // THE ACTUAL SIMULATION
                for (int cIterations = 1; cIterations <= Iterations; cIterations++)
                {
                    Console.WriteLine("Iteration " + cIterations + " out of " + Iterations);
                    Adults.Clear();
                    Eggs.Clear();

                    if (ApplyIntervention)
                        Populate_with_WT();
                    else
                        Populate_with_Setup();

                    Shuffle.ShuffleList(Adults);

                    for (int cGenerations = 1; cGenerations <= Generations; cGenerations++)
                    {
                        if (ApplyIntervention)
                        {
                            if ((cGenerations >= StartIntervention) && (cGenerations <= EndIntervention))
                            {
                                Intervention();
                            }
                            else
                                if ((cGenerations >= StartInterventionsecond) && (cGenerations <= EndInterventionsecond))
                                {
                                
                                    Intervention();
                                }

                        }
                        #region output data to file

                        //------------------------ Genotypes -------

                        List<string> Genotypes = new List<string>();

                         foreach (Organism O in Adults)
                         {
                             foreach (string s in White_Track)
                             {
                                Genotypes.Add(s + "," + O.GetGenotype(s));
                             }
                         }

                        var queryG = Genotypes.GroupBy(s => s)
                           .Select(g => new { Name = g.Key, Count = g.Count() });

                        foreach (var result in queryG)
                        {
                          Fwriter.WriteLine("{0},{1},{2},{3}", cIterations, cGenerations, result.Name, result.Count);

                        }

                        Genotypes.Clear();

                        //   
                       /* List<string> NGenotypes = new List<string>();

                        foreach (Organism O in Adults)
                        {
                            foreach (string s in Nudel_Track)
                            {
                                NGenotypes.Add(s + "," + O.GetGenotype(s));
                            }
                        }

                        var NqueryG = NGenotypes.GroupBy(s => s)
                           .Select(g => new { Name = g.Key, Count = g.Count() });

                        foreach (var result in NqueryG)
                        {
                            Fwriter.WriteLine("{0},{1},{2},{3},all", cIterations, cGenerations, result.Name, result.Count);
                        }

                        NGenotypes.Clear();
                        /// */

                        
                        int cSample = Sample;
                        foreach (Organism O in Adults)
                        {
                            if (cSample > 0)
                            {
                                foreach (string s in White_Track)
                                {
                                    Genotypes.Add(s + "," + O.GetGenotype(s));
                                }
                                cSample--;
                            }
                        }

                        var queryGs = Genotypes.GroupBy(s => s)
                           .Select(g => new { Name = g.Key, Count = g.Count() });

                        foreach (var result in queryGs)
                        {
                            Fwriter.WriteLine("{0},{1},{2},{3}", cIterations, cGenerations, result.Name, result.Count);
                        }

                        //------------------------- Sex -----------
                        int numberofallmales = 0;
                        int numberofallfemales = 0;
                        foreach (Organism O in Adults)
                        {
                            if (O.GetSex() == "female")
                                numberofallfemales++;
                            else
                                numberofallmales++;
                        }
                        Fwriter.WriteLine("{0},{1},{2},{3},{4},{5}", cIterations, cGenerations, "Sex", "Males", "NA", numberofallmales);
                        Fwriter.WriteLine("{0},{1},{2},{3},{4},{5}", cIterations, cGenerations, "Sex", "Females", "NA", numberofallfemales);

                        //------------------------- Sex Karyotype -----------
                        int numberofXX = 0;
                        int numberofXY = 0;
                        foreach (Organism O in Adults)
                        {

                            switch (O.GetSexChromKaryo())
                            {
                                case "XX":
                                    {
                                        numberofXX++;
                                        break;
                                    }
                                case "XY":
                                    {
                                        numberofXY++;
                                        break;
                                    }
                                case "YX":
                                    {
                                        numberofXY++;
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine(O.GetSexChromKaryo() + " should not exist!");
                                        break;
                                    }
                            }

                        }
                       // Fwriter.WriteLine("{0},{1},{2},{3},{4},{5},{6}", cIterations, cGenerations, "Sex_Karyotype", "XX", "NA", numberofXX, "all");
                       // Fwriter.WriteLine("{0},{1},{2},{3},{4},{5},{6}", cIterations, cGenerations, "Sex_Karyotype", "XY", "NA", numberofXY, "all");



                        #endregion

                        Shuffle.ShuffleList(Adults);
                        CrossAll();
                        Adults.Clear();
                        Shuffle.ShuffleList(Eggs);

                        #region Return Adults from Eggs for the next Generation
                        int EggsToBeReturned = 0;

                        if (Eggs.Count <= PopulationCap)
                            EggsToBeReturned = Eggs.Count;
                        else
                            EggsToBeReturned = PopulationCap;

                        for (int na = 0; na < EggsToBeReturned; na++)
                        {
                            Adults.Add(new Organism(Eggs[na]));
                        }
                        #endregion

                        Fwriter.WriteLine("{0},{1},{2},{3},{4},{5}", cIterations, cGenerations, "Eggs", "NA", "NA", Eggs.Count.ToString());

                        Eggs.Clear();

                    }

                }
                // END OF SIMULATION

                Fwriter.Flush();
            }
        }

        //---------------------- Define Organisms, Genotypes and Starting Populations -----------------------------------------------------

        public void Populate_with_WT()
        {
            for (int i = 0; i < StartingNumberOfWTFemales; i++)
            {
                Adults.Add(new Organism(GenerateWTFemale()));
            }
            for (int i = 0; i < StartingNumberOfWTMales; i++)
            {
                Adults.Add(new Organism(GenerateWTMale()));
            }
            for (int i = 0; i < 50; i++)
            {
                Adults.Add(new Organism(Generate_NudelMale()));
            }
            for (int i = 0; i < 50; i++)
            {
                Adults.Add(new Organism(Generate_DriveFemale()));
            }
        }

        public void Populate_with_Setup()
        {
            for (int i = 0; i < 50; i++)
            {
                Adults.Add(new Organism(GenerateWTFemale()));
            }
            for (int i = 0; i < 100; i++)
            {
                Adults.Add(new Organism(GenerateWTMale()));
            }

            for (int i = 0; i < 15; i++)
            {
                Adults.Add(new Organism(Generate_DriveMale()));
            }

            for (int i = 0; i < 50; i++)
            {
                Adults.Add(new Organism(Generate_DriveFemale()));
            }
            for (int i = 0; i < 0; i++)
            {
                Adults.Add(new Organism(Generate_NudelMale()));
            }

            for (int i = 0; i < 0; i++)
            {
                Adults.Add(new Organism(Generate_NudelFemale()));
            }
        }

        public void Intervention()
        {
           

            for (int i = 0; i < White_drive_male; i++)
            {
                Adults.Add(new Organism(Generate_DriveMale()));
            }
             for (int i = 0; i < White_drive_female; i++)
             {
                 Adults.Add(new Organism(Generate_DriveFemale()));
             }
            for (int i = 0; i < Nudel_male; i++)
            {
                Adults.Add(new Organism(Generate_NudelMale()));
            }
            for (int i = 0; i < Nudel_female; i++)
            {
                Adults.Add(new Organism(Generate_NudelFemale()));
            }
           
        }

        public Organism GenerateWTFemale()
        {
            Organism WTFemale = new Organism();

            GeneLocus Whitea = new GeneLocus("White", 1, "WT");
            Whitea.Traits.Add("Conservation", 10.01F);
            Whitea.Traits.Add("Hom_Repair", 0.95F);
            GeneLocus Whiteb = new GeneLocus("White", 1, "WT");
            Whiteb.Traits.Add("Conservation", 10.01F);
            Whiteb.Traits.Add("Hom_Repair", 0.95F);

            GeneLocus Nudela = new GeneLocus("Nudel", 1, "WT");
            Nudela.Traits.Add("Conservation", 10.01F);
            Nudela.Traits.Add("Hom_Repair", 0.95F);
            GeneLocus Nudelb = new GeneLocus("Nudel", 1, "WT");
            Nudelb.Traits.Add("Conservation", 10.01F);
            Nudelb.Traits.Add("Hom_Repair", 0.95F);


            Chromosome ChromXa = new Chromosome("X", "Sex");
            Chromosome ChromXb = new Chromosome("X", "Sex");
            Chromosome Chrom2a = new Chromosome("2", "2");
            Chromosome Chrom2b = new Chromosome("2", "2");
            Chromosome Chrom3a = new Chromosome("3", "3");
            Chromosome Chrom3b = new Chromosome("3", "3");

            Chrom2a.GeneLocusList.Add(Whitea);
            Chrom2b.GeneLocusList.Add(Whiteb);
            Chrom3a.GeneLocusList.Add(Nudela);
            Chrom3b.GeneLocusList.Add(Nudelb);

            WTFemale.ChromosomeListA.Add(ChromXa);
            WTFemale.ChromosomeListB.Add(ChromXb);
            WTFemale.ChromosomeListA.Add(Chrom2a);
            WTFemale.ChromosomeListB.Add(Chrom2b);
            WTFemale.ChromosomeListA.Add(Chrom3a);
            WTFemale.ChromosomeListB.Add(Chrom3b);

            return WTFemale;
        }

        public Organism GenerateWTMale()
        {
            Organism WTMale = new Organism(GenerateWTFemale());
            Chromosome ChromY = new Chromosome("Y", "Sex");
            GeneLocus MaleFactor = new GeneLocus("MaleDeterminingLocus", 1, "WT");
            ChromY.GeneLocusList.Add(MaleFactor);

            WTMale.ChromosomeListA[0] = ChromY;
            return WTMale;
        }

        public Organism Generate_DriveMale()
        {
            Organism IDG_Male = new Organism(GenerateWTMale());

            GeneLocus IDG = new GeneLocus("White", 1, "Construct");
            IDG.Traits.Add("transgene_Cas9", 95);
            IDG.Traits.Add("transgene_White_gRNA", 1);
            IDG.Traits.Add("Hom_Repair", 0.096F);
            /*83zf
             84nf
            85vf
            50vm
            11nm
            9.6zm
            */

            Organism.ModifyAllele(ref IDG_Male.ChromosomeListA, IDG, "WT");
            return IDG_Male;           
        }
        public Organism Generate_DriveFemale()
        {
            Organism IDG_Female = new Organism(GenerateWTFemale());

            GeneLocus IDG = new GeneLocus("White", 1, "Construct");
            IDG.Traits.Add("transgene_Cas9", 95);
            IDG.Traits.Add("transgene_White_gRNA", 1);
            IDG.Traits.Add("Hom_Repair", 0.83F);

            Organism.ModifyAllele(ref IDG_Female.ChromosomeListB, IDG, "WT");
            return IDG_Female;
        }
        public Organism Generate_NudelMale()
        {
            Organism IDG_NMale = new Organism(GenerateWTMale());

            GeneLocus IDG = new GeneLocus("Nudel", 1, "Construct");
            IDG.Traits.Add("transgene_Nudel_gRNA", 1);
            IDG.Traits.Add("Hom_Repair", 0.7362F);

            Organism.ModifyAllele(ref IDG_NMale.ChromosomeListA, IDG, "WT");
            Organism.ModifyAllele(ref IDG_NMale.ChromosomeListB, IDG, "WT");
            return IDG_NMale;
        }
        public Organism Generate_NudelFemale()
        {
            Organism IDG_NFemale = new Organism(GenerateWTFemale());

            GeneLocus IDG = new GeneLocus("Nudel", 1, "Construct");
            IDG.Traits.Add("transgene_Nudel_gRNA", 1);
            IDG.Traits.Add("Hom_Repair", 0.8553F);

            Organism.ModifyAllele(ref IDG_NFemale.ChromosomeListB, IDG, "WT");
            return IDG_NFemale;
        }

        //----------------------- Simulation methods ----------------------------------------------------


        public void PerformCross(Organism Dad, Organism Mum, ref List<Organism> EggList)
        {
            int EggsPerFemale = GlobalEggsPerFemale;

            if (eggchance <= (float)Simulation.random.NextDouble())
            {EggsPerFemale = 0; }
             else
            EggsPerFemale = (int)(EggsPerFemale * Dad.GetFertility() * Mum.GetFertility());


            for (int i = 0; i < EggsPerFemale; i++)
                {
                    EggList.Add(new Organism(Dad,Mum));
                }
        }
        public void CrossAll()
        {

            int EffectivePopulation = (int)((1 - Mortality) * PopulationCap);
  
            int numb;
            foreach (Organism F1 in Adults)
            {
                if (F1.GetSex() == "male")
                {
                    continue;
                }
                else
                {
                    for (int a = 0; a < EffectivePopulation; a++)
                    {
                        numb = random.Next(0, Adults.Count);
                        if (Adults[numb].GetSex() == "male")
                        {
                            PerformCross(Adults[numb], F1, ref Eggs);
                            break;
                        }
                    }
                }

            }

        }

    }
}
