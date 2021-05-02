using System;
using System.Threading;

namespace crossing1
{


    class Road
    {
        //pola poczatkowe               top         right       bottom      left
        int[,] startFields = new int[,] { { 12, 1 }, { 23, 12 }, { 13, 23 }, { 1, 13 } };
        //pola koncowe             // endTop      right    bottom      left
        int[,] endFields = new int[,] { { 13, 1 }, { 23, 13 }, { 12, 23 }, { 1, 12 } };

        //pola intersection
        //  x1
        //   A B x2
        //   x3  C D
        //        x4
        int[,] crossInner = new int[,] { { 12, 12 }, { 13, 12 }, { 12, 13 }, { 13, 13 } };
        //x1,x2,x3,x4
        int[,] crossOuter = new int[,] { { 12, 11 }, { 14, 12 }, { 11, 13 }, { 13, 14 } };
        bool[,] roadSpace = new bool[25, 25];
        Mutex[,] roadSpaceMutex = new Mutex[25, 25];

        public Road()
        {
            for (int i = 0; i < 25; i++)
            {
                //roadSpaceMutex[i] = new Mutex[25];
                // roadSpace[i] = new bool[25];
                for (int j = 0; j < 25; j++)
                {
                    roadSpaceMutex[i, j] = new Mutex();
                    roadSpace[i, j] = true;
                }
            }
        }

        public Mutex GetRoadMutex(int pos_x, int pos_y)
        {
            return roadSpaceMutex[pos_x, pos_y];
        }
        //return true in "standard" case
        public bool straightRoad(int pos_x, int pos_y)
        {
            for (int i = 0; i < 4; i++)
            {
                if (pos_x == crossInner[i, 0] && pos_y == crossInner[i, 1])
                {
                    return false;
                }
                if (pos_x == crossOuter[i, 0] && pos_y == crossOuter[i, 1])
                {
                    return false;
                }
            }
            return true;
        }
        //sprawdz czy podane kaordynaty sa wolne true-mozna jechac, flase- nie mozna
        //need to be in safe block of code
        // GetRoadMutex(x,y).WaitOne();
        // :::::::::::
        // GetRoadMutex(x,y).ReleaseMuex();
        public bool checkSpace(int pos_x, int pos_y)
        {
            //musze blokowac to w wiekszym zakresie
            //roadSpace[pos_x][pos_y].WaitOne();
            //bool temp=
            return roadSpace[pos_x, pos_y];
        }
        //need to be in safe block of code
        // GetRoadMutex(x,y).WaitOne();
        // :::::::::::
        // GetRoadMutex(x,y).ReleaseMuex();
        public void setSpaceOccupied(int pos_x, int pos_y)
        {
            roadSpace[pos_x, pos_y] = false;
        }
        //need to be in safe block of code
        // GetRoadMutex(x,y).WaitOne();
        // :::::::::::
        // GetRoadMutex(x,y).ReleaseMuex();
        public void setSpaceFree(int pos_x, int pos_y)
        {
            roadSpace[pos_x, pos_y] = true;
        }

        //to chyba nie musi byc w bezpiecznej skecji
        public bool checkIfEnd(int pos_x, int pos_y)
        {
            for (int i = 0; i < 4; i++)
            {
                //check             0-x                     1-y
                if (pos_x == endFields[i, 0] && pos_y == endFields[i, 1])
                {
                    return true;
                }

            }
            return false;
        }

    }
}