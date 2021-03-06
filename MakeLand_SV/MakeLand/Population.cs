﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Media;

namespace MakeLand
{
    public class Population
    {
        public int generation = 0;
        public int bestScore = 0;
        public int bestIndex = 0;
        public int numInPop = 0;

        public int[] listOfLiving;
        public int countOfLiving;

        public Phenotype[] maps = null;

        public Population(int numInPopZ, Random r)
        {
            numInPop = numInPopZ;
            maps = new Phenotype[numInPop];
            for (int i = 0; i < numInPop; i++)
            {
                Genotype g = new Genotype(r);
                Phenotype p = new Phenotype(g,0);
                p.createPheno();
                p.setScore();
                
                maps[i] = p; 
            }
        }

        /// <summary>
        /// Returns the index of the best individual and updates bestScore
        /// </summary>
        /// <returns></returns>
        public int findBest()
        {
            Phenotype p = maps[0];
            bestScore = p.score;
            bestIndex = 0;
            for (int i = 1; i < numInPop; i++)
            {
                p = maps[i];
                if (p.score > bestScore)
                {
                    bestIndex = i;
                    bestScore = p.score;
                }
            }
            return bestIndex;
        }


        /// <summary>
        /// Finds the worst individual thats actually alive
        /// </summary>
        /// <returns></returns>
        public int findWorstAlive()
        {
            bool first = true;
            int worstScore = 0;
            int worstIndex = 0;

            for (int i = 1; i < numInPop; i++)
            {
                Phenotype p = maps[i];
                if (p.alive && first)
                {
                    first = false;
                    worstScore = p.score;
                    worstIndex = i;
                    continue;
                }

                if (p.alive && p.score < worstScore)
                {
                    worstScore = p.score;
                    worstIndex = i;
                }
            }
            return worstIndex;
        }

        /// <summary>
        /// Just a standard getter
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Phenotype getPhenotype(int i)
        {
            return maps[i];
        }

        /// <summary>
        /// Unsets the newborn flag for the entire population
        /// </summary>
        public void unsetNewborn()
        {
            for (int i = 1; i < numInPop; i++)
            {
                getPhenotype(i).newborn = false;
            }
        }

        /// <summary>
        /// Kills the weakest
        /// </summary>
        /// <param name="n"></param>
        public void killThisMany(int n)
        {

            for (int i = 0; i <n; i++)
            {
                int k = findWorstAlive();
                getPhenotype(k).alive = false;
            }
        }



        /// <summary>
        /// Search for dead individuals - replace them with living newborn ones
        /// </summary>
        public void breedPopulation(Random r)
        {
            listOfLiving = new int[Params.populationCnt];
            countOfLiving=0;
            for (int i = 0; i < Params.populationCnt; i++)
            {
                if (getPhenotype(i).alive && (!getPhenotype(i).newborn))
                {
                    listOfLiving[i] = i;
                    countOfLiving++;
                }
            }

            for (int i = 0; i < Params.populationCnt; i++)
            {
                if (!getPhenotype(i).alive)
                {
                    int mum = r.Next(0, countOfLiving);
                    int dad = r.Next(0, countOfLiving);
                    mum = listOfLiving[mum];
                    dad = listOfLiving[dad];
                    Phenotype mumP = getPhenotype(mum);
                    Phenotype dadP = getPhenotype(dad);
                    Genotype ggg = makeGenome(mumP.genotype,dadP.genotype);
                    if (Params.mutationPercent > r.Next(0,100)) mutate(ggg, r);
                    //checkDuplicateGenes(ggg);
                    maps[i] = new Phenotype(ggg, G.pop.generation);

                }
            }
        }


        public bool checkDuplicateGenes(Genotype ggg)
        {
            bool retv = false;
            for (int i = 0; i < Params.genotypeSize; i++)
                for (int k = i+1; k < Params.genotypeSize; k++)
                {
                    if(ggg.genes[i].equal(ggg.genes[k]))
                      {
                        G.dupGeneCount++;
                        ggg.genes[i] = new Gene(G.rnd);
                        retv = true;
                      }
                }
            return retv;
        }

        public void mutate(Genotype g, Random r)
        {
            int mutateType = 4;
            int mutatecase1 = 100/mutateType;
            int mutatecase2 = 100/mutateType;
            int mutatecase3 = 100 / mutateType;
            int mutatecase4 = 100 / mutateType;
            int mut100 = r.Next(0, 100);
            if (mut100 < mutatecase1) {mutateType = 1;} else
            if (mut100 < mutatecase1 + mutatecase2) { mutateType = 2;}else
            if (mut100 < mutatecase1 + mutatecase2+mutatecase3) { mutateType = 3; }else
            if (mut100 < mutatecase1 + mutatecase2 + mutatecase3+mutatecase4) { mutateType = 4; }
            switch (mutateType)
            {

                //create a random gene
                case 1:
                    int t = r.Next(0, Params.genotypeSize);
                    for (int jj = 0; jj < t; jj++)
                    {
                        int i = r.Next(0, Params.genotypeSize);
                        g.genes[i] = new Gene(r);
                    }
                    G.mutationCount++;
                    mutatecase1++;
                    break;

                //swap 2 random genes
                case 2:
                    int ii = r.Next(0, Params.genotypeSize);
                    int kk = r.Next(0, Params.genotypeSize);
                    Gene hold = g.genes[kk];
                    g.genes[kk] = g.genes[ii];
                    g.genes[ii] = hold;
                    G.mutationCount++;
                    mutatecase2++;
                    break;

                //change 1 terrain to another terrain
                case 3:
                    int w = r.Next(0, Params.genotypeSize);
                    for (int jj = 0; jj < w; jj++)
                    {
                        int i = r.Next(0, Params.genotypeSize);

                        if (g.genes[i].terrain == 2)
                        {
                            g.genes[i].terrain = 1;
                        }


                        if (g.genes[i].terrain == 0)
                        {
                            g.genes[i].terrain = 1;
                        }

                    }
                    G.mutationCount++;
                    mutatecase3++;
                    break;

                //check the area of the genes
                case 4:
                    int land = r.Next(40, Params.dimY - 20);
                    int outlandx = r.Next(0, 20);
                    int outlandy = r.Next(Params.dimY - 20, Params.dimY);
                                int q = r.Next(0, Params.genotypeSize);
                                for (int jj = 0; jj < q; jj++)
                                {                                   
                                    int i = r.Next(0, Params.genotypeSize);
                            //check the miidle area to find out if gene is water, if it is, then move it out of the middle area        
                            if ((g.genes[i].x < Params.dimX - 20 && g.genes[i].x > 20)&&(g.genes[i].y < Params.dimY - 20 && g.genes[i].y > 20))
                            {
                            if (g.genes[i].terrain == 0)
                            {
                                g.genes[i].x = outlandx;
                                g.genes[i].y = outlandy;
                            }
                            }
                            //check the genes around the middle area, then check their terrain, if it is 0 then move it in the middle and turn it into 1,
                            // if terrain is 1 then move it in the middle, if terrain is 2 then move it in the middle
                            if((((g.genes[i].x > Params.dimX - 20)||(g.genes[i].x < 20)) && (g.genes[i].y > Params.dimY-20)))
                        {
                            if (g.genes[i].terrain == 0)
                            {
                                g.genes[i].terrain = 1;
                                g.genes[i].x = land;
                                g.genes[i].y = land;
                            }
                            if (g.genes[i].terrain == 1)
                            {
                                g.genes[i].x = land;
                                g.genes[i].y = land;
                            }
                            if (g.genes[i].terrain == 2)
                            {
                                g.genes[i].x = land;
                                g.genes[i].y = land;
                            }

                        }
                            if ((g.genes[i].y > Params.dimY - 20) || ((g.genes[i].y < 20) || (g.genes[i].x > Params.dimY - 20)))
                        {
                            if (g.genes[i].terrain == 0)
                            {
                                g.genes[i].terrain = 1;
                                g.genes[i].x = land;
                                g.genes[i].y = land;
                            }
                            if (g.genes[i].terrain == 1)
                            {
                                g.genes[i].x = land;
                                g.genes[i].y = land;
                            }
                            if (g.genes[i].terrain == 2)
                            {
                                g.genes[i].x = land;
                                g.genes[i].y = land;
                            }
                        }
                    }                 
                    G.mutationCount++;
                    mutatecase4++;
                    break;
            }                
    }
        /// <summary>
        /// create a new geneome from mum and dad
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public Genotype makeGenome(Genotype g1, Genotype g2)
        {
            Genotype retv = new Genotype();
            for (int i = 0; i < Params.genotypeSize; i++)
            {
                if (G.rnd.NextDouble()<0.5)
                {
                    retv.genes[i] = new Gene(g1.genes[i]);
                }
                else
                {
                    retv.genes[i] = new Gene(g2.genes[i]);
                }
            }
            return retv;
        }

        public void checkDuplicateGenotypes()
        {
            for (int i = 0; i < Params.populationCnt; i++)
            {
                Genotype g = getPhenotype(i).genotype;
                if (checkDuplicateGenes(g)) continue;
                for (int k = i + 1; k < Params.populationCnt; k++)
                {
                    Genotype kk = getPhenotype(k).genotype;
                    if (kk.equal(g))
                    {
                        mutate(g, G.rnd);
                        G.dupGeneomeCount++;
                    }
                }
            }
        }


        /// <summary>
        /// what it sounds like
        /// </summary>
        public void do1Generation()
        {
           
            G.pop.generation++;
            unsetNewborn();
            killThisMany(Params.populationCnt / 2);
            breedPopulation(G.rnd);
            if (G.pop.generation % 10 == 0)
            {
                
                for (int re = 0; re < Params.populationCnt; re++)
                {
                    Genotype g = getPhenotype(re).genotype;
                    if (checkDuplicateGenes(g)) checkDuplicateGenotypes() ;
                    
                }
            }
            //if (Params.checkDuplicateGenomes != -1 && G.pop.generation % Params.checkDuplicateGenomes == 0) checkDuplicateGenotypes(); 
            Application.DoEvents();
        }

    }

    public class Genotype
    {
        public Gene[] genes = new Gene[Params.genotypeSize];

        public Genotype(Random r)
        {
            for (int i = 0; i < Params.genotypeSize; i++)
                genes[i] = new Gene(r);
        }

        public Genotype()
        {
            for (int i = 0; i < Params.genotypeSize; i++)
                genes[i] = new Gene();
        }

        public bool equal(Genotype gg)
        {
            for (int i = 0; i < Params.genotypeSize; i++)
            {
                if (!(gg.genes[i].equal(genes[i]))) return false;
            }
            return true;
        }
    }



    public class Gene
    {
        public int terrain=0;
        public int x=0;
        public int y=0;
        public int repeatY = 0;
        public int repeatX = 0;

        public Gene()
        {

        }

        public Gene(int ter, int xx, int yy, int rptX, int rptY)
        {
            terrain = ter;
            x = xx;
            y = yy;
            repeatX = rptX;
            repeatY = rptY;
        }

        /// <summary>
        /// New Random Gene
        /// </summary>
        /// <param name="r"></param>
        public Gene(Random r)
        {
            terrain = r.Next(0,3);
            x = r.Next(0, Params.dimX);
            y = r.Next(0, Params.dimY);
            repeatX = r.Next(0, Params.maxRepeat);
            repeatY = r.Next(0, Params.maxRepeat);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="gg"></param>
        public Gene(Gene gg) // copy constructor
        {
            terrain = gg.terrain;
            x = gg.x;
            y = gg.y;
            repeatX = gg.repeatX;
            repeatY = gg.repeatY;
        }

        public bool equal(Gene g)
        {
            if (g.x != x) return false;
            if (g.y != y) return false;
            if (g.repeatX != repeatX) return false;
            if (g.repeatY != repeatY) return false;
            if (g.terrain != terrain) return false;
            return true;
        }

    }

    public class Phenotype
    {
        public Genotype genotype=null; // reference class - this is a pointer not a copy
        int[,] pheno = null;
        Bitmap bitm = null;
        public int score = 0;
        public double seaCount = 0;
        public double landCount = 0;
        public double freshCount = 0;
        public double totalCount = 0;
        public bool alive = true;
        public bool newborn = true;
        public bool landover = false;
        public bool seaover = false;
        public bool freshover = false;
        public int gen = 0; 

        /// <summary>
        /// Default constructor probably not helpfull
        /// </summary>
        public Phenotype()
        {
            // default is all null - no need for code yet
        }

        /// <summary>
        /// This is the critical constructor it creates the pheno array for scoring
        /// </summary>
        /// <param name="gg"></param>
        public Phenotype(Genotype gg, int generationCount)
        {
            genotype = gg;
            createPheno();
            setScore();
            gen = generationCount;

        }

        /// <summary>
        ///  create the pheno array
        /// </summary>
        public void createPheno()
        {
            pheno = new int[Params.dimX, Params.dimY];
            for (int x=0; x< Params.dimX;x++)
                for (int y=0; y< Params.dimY;y++) { pheno[x,y] = 0; } // initialise to 0

            for (int i = 0; i < Params.genotypeSize; i++)
            {
                Gene g = genotype.genes[i];
                for (int kx = 0; kx < g.repeatX; kx++)
                    for (int ky = 0; ky < g.repeatY; ky++)
                    {
                        int x = g.x+kx;
                        int y = g.y+ky;
                    if (y< Params.dimY && x< Params.dimX) pheno[x, y] = g.terrain;
                }

            }
        }

        public int getTerrainSafe(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Params.dimX || y >= Params.dimY) return 0;
            return pheno[x, y];
        }

        /// <summary>
        /// returns the score for selection - also stores it in Phenotype
        /// </summary>
        /// <returns></returns>
        public int setScore()
        {          
            score = 0;
            int every = 1;
            seaCount = 0;
            landCount = 0;
            freshCount = 0;
            totalCount = 0;
            Random r = new Random();
            int rnd = 0;
            rnd = r.Next(1, 300);

            for (int x = 0; x < Params.dimX; x = x + every)
            {
                for (int y = 0; y < Params.dimY; y = y + 1)
                {
                    //calculate the ratio
                    if (getTerrainSafe(x, y) == 0)
                    { seaCount++; }
                    if (getTerrainSafe(x, y) == 1)
                    { landCount++; }
                    if (getTerrainSafe(x, y) == 2)
                    { freshCount++; }

                    int gap = 15;
                    //check if sea is near the edge
                    if ((x < Params.dimX && y < gap) || (x < Params.dimX && y > Params.dimY - gap))
                         {
                        if (getTerrainSafe(x, y) == 2) { score--; }
                        if (getTerrainSafe(x, y) == 0) { score++; }
                        if (getTerrainSafe(x, y) == 1) { score--; }
                    }                    
                    else if((y < Params.dimY && x < gap) || (y < Params.dimY && x > Params.dimX - gap))
                    {
                        if (getTerrainSafe(x, y) == 2) { score--; }
                        if (getTerrainSafe(x, y) == 0) { score++; }
                        if (getTerrainSafe(x, y) == 1) { score--; }
                    }

                    else {

                        //check if land and freshwater are in the middle

                        if ((y < Params.dimY && x > gap) || (y < Params.dimY && x < Params.dimX - gap))
                        {
                            if (getTerrainSafe(x, y) == 2) { score++; }
                            if (getTerrainSafe(x, y) == 0) { score--; }
                            if (getTerrainSafe(x, y) == 1) { score++; }
                        }
                        if ((x < Params.dimX && y > gap) || (y < Params.dimX && y < Params.dimX - gap))
                        {
                            if (getTerrainSafe(x, y) == 2) { score++; }
                            if (getTerrainSafe(x, y) == 0) { score--; }
                            if (getTerrainSafe(x, y) == 1) { score++; }
                        }
                        
                        //if cell is land     
                        if (getTerrainSafe(x, y) == 1)
                            {

                                //checking if land ratio is > 70%
                                if (landCount / totalCount < Params.percentLand)
                                {
                                    //surround by land
                                if (getTerrainSafe(x - 1, y - 1) == 1) { score++; }
                                if (getTerrainSafe(x, y - 1) == 1) { score++; }
                                if (getTerrainSafe(x + 1, y - 1) == 1) { score++; }
                                if (getTerrainSafe(x - 1, y) == 1){ score++; }
                                if (getTerrainSafe(x + 1, y) == 1) { score++; }
                                if (getTerrainSafe(x - 1, y + 1) == 1) { score++; }
                                if (getTerrainSafe(x, y + 1) == 1) { score++; }
                                if (getTerrainSafe(x + 1, y + 1) == 1) { score++; }
                                }

                                //check if land ratio < 70%
                                if (landCount / totalCount > Params.percentLand)
                                {
                                    //surround by water
                                if (getTerrainSafe(x - 1, y - 1) == 0) { score--; }
                                if (getTerrainSafe(x, y - 1) == 0) { score--; }
                                if (getTerrainSafe(x + 1, y - 1) == 0) { score--; }
                                if (getTerrainSafe(x - 1, y) == 0) { score--; }
                                if (getTerrainSafe(x + 1, y) == 0) { score--; }
                                if (getTerrainSafe(x - 1, y + 1) == 0) { score--; }
                                if (getTerrainSafe(x, y + 1) == 0) { score--; }
                                if (getTerrainSafe(x + 1, y + 1) == 0) { score--; }
                                }

                                //check if sea >26%
                                if (seaCount / totalCount > 1 - (Params.percentFresh + Params.percentLand))
                                {
                                    //surround by freshwater
                                if (getTerrainSafe(x - 1, y - 1) == 2) { score++; }
                                if (getTerrainSafe(x, y - 1) == 2) { score++; }
                                if (getTerrainSafe(x + 1, y - 1) == 2) { score++; }
                                if (getTerrainSafe(x - 1, y) == 2) { score++; }
                                if (getTerrainSafe(x + 1, y) == 2) { score++; }
                                if (getTerrainSafe(x - 1, y + 1) == 2) { score++; }
                                if (getTerrainSafe(x, y + 1) == 2) { score++; }
                                if (getTerrainSafe(x + 1, y + 1) == 2) { score++; }
                            }
                            }
                            //cell is water
                            if (getTerrainSafe(x, y) == 0)
                            {

                                //check if water ratio is less than 26%
                                if (seaCount / totalCount < (1 - (Params.percentFresh + Params.percentLand)))
                                //surround by water
                                {
                                if (getTerrainSafe(x - 1, y - 1) == 0) { score++; }
                                if (getTerrainSafe(x, y - 1) == 0) { score++; }
                                if (getTerrainSafe(x + 1, y - 1) == 0) { score++; }
                                if (getTerrainSafe(x - 1, y) == 0) { score++; }
                                if (getTerrainSafe(x + 1, y) == 0) { score++; }
                                if (getTerrainSafe(x - 1, y + 1) == 0) { score++; }
                                if (getTerrainSafe(x, y + 1) == 0) { score++; }
                                if (getTerrainSafe(x + 1, y + 1) == 0) { score++; }
                            }
                                //check if sea > 26%
                            if (seaCount / totalCount > (1 - (Params.percentFresh + Params.percentLand)))
                            {
                                //surround by freshwater
                                if (getTerrainSafe(x - 1, y - 1) == 2) { score--; }
                                if (getTerrainSafe(x, y - 1) == 2) { score--; }
                                if (getTerrainSafe(x + 1, y - 1) == 2) { score--; }
                                if (getTerrainSafe(x - 1, y) == 2) { score--; }
                                if (getTerrainSafe(x + 1, y) == 2) { score--; }
                                if (getTerrainSafe(x - 1, y + 1) == 2) { score--; }
                                if (getTerrainSafe(x, y + 1) == 2) { score--; }
                                if (getTerrainSafe(x + 1, y + 1) == 2) { score--; }
                            }

                            //check if land ratio is less than 70%
                            if (landCount / totalCount < Params.percentLand)
                            {
                                    //surround by land
                                if (getTerrainSafe(x - 1, y - 1) == 1) { score--; }
                                if (getTerrainSafe(x, y - 1) == 1) { score--; }
                                if (getTerrainSafe(x + 1, y - 1) == 1) { score--; }
                                if (getTerrainSafe(x - 1, y) == 1) { score--; }
                                if (getTerrainSafe(x + 1, y) == 1) { score--; }
                                if (getTerrainSafe(x - 1, y + 1) == 1) { score--; }
                                if (getTerrainSafe(x, y + 1) == 1) { score--; }
                                if (getTerrainSafe(x + 1, y + 1) == 1) { score--; }
                            }

                        }
                            //cell is fresh water
                            if (getTerrainSafe(x, y) == 2)
                            {

                            //check if land ratio is less than 70%
                            if (landCount / totalCount < Params.percentLand)
                            {
                                //surround by land
                                if (getTerrainSafe(x - 1, y - 1) == 1) { score++; }
                                if (getTerrainSafe(x, y - 1) == 1) { score++; }
                                if (getTerrainSafe(x + 1, y - 1) == 1) { score++; }
                                if (getTerrainSafe(x - 1, y) == 1) { score++; }
                                if (getTerrainSafe(x + 1, y) == 1) { score++; }
                                if (getTerrainSafe(x - 1, y + 1) == 1) { score++; }
                                if (getTerrainSafe(x, y + 1) == 1) { score++; }
                                if (getTerrainSafe(x + 1, y + 1) == 1) { score++; }
                            }

                            //check if water ratio is less than 26%
                            if (freshCount / totalCount > Params.percentFresh)
                            //surround by water
                            {
                                if (getTerrainSafe(x - 1, y - 1) == 0) { score--; }
                                if (getTerrainSafe(x, y - 1) == 0) { score--; }
                                if (getTerrainSafe(x + 1, y - 1) == 0) { score--; }
                                if (getTerrainSafe(x - 1, y) == 0) { score--; }
                                if (getTerrainSafe(x + 1, y) == 0) { score--; }
                                if (getTerrainSafe(x - 1, y + 1) == 0) { score--; }
                                if (getTerrainSafe(x, y + 1) == 0) { score--; }
                                if (getTerrainSafe(x + 1, y + 1) == 0) {score--;}
                            }

                            //check if the freshwater ratio is more than 4%
                            if (freshCount / totalCount < Params.percentFresh)
                                {
                                    //surround by freshwater
                                if (getTerrainSafe(x - 1, y - 1) == 2) { score++; }
                                if (getTerrainSafe(x, y - 1) == 2) { score++; }
                                if (getTerrainSafe(x + 1, y - 1) == 2) { score++; }
                                if (getTerrainSafe(x - 1, y) == 2) { score++; }
                                if (getTerrainSafe(x + 1, y) == 2) { score++; }
                                if (getTerrainSafe(x - 1, y + 1) == 2) { score++; }
                                if (getTerrainSafe(x, y + 1) == 2) { score++; }
                                if (getTerrainSafe(x + 1, y + 1) == 2) { score++; }
                            }
                        }
                    }
                    totalCount = landCount + freshCount + seaCount;
                }
            }           
            return 0;
        }

        /// <summary>
        /// Display the map in a picturebox
        /// </summary>
        public void show(PictureBox pb)
        {
            System.Drawing.SolidBrush myBrush;
            if (bitm == null)
            {
                bitm = new Bitmap(Params.dimX, Params.dimY);
                myBrush = new System.Drawing.SolidBrush(G.ca[0]);
                Graphics gra = Graphics.FromImage(bitm);

                gra.FillRectangle(myBrush,0,0, Params.dimX, Params.dimY); //this is your code for drawing rectangles
                
                for (int x=0; x< Params.dimX; x++)
                {
                    for (int y = 0; y < Params.dimY; y++)
                    {
                        if (pheno[x,y] > 0)
                        {
                            bitm.SetPixel(x, y, G.ca[pheno[x,y]]);
                        }
                    }
                }
            }
            pb.Image = bitm;
        }
    }
}
