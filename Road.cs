using System;
using System.Threading;
using System.Collections.Generic;//queue
namespace crossing1
{


    class Road
    {
        //pola poczatkowe               top         right       bottom      left
        int[,] startFields = new int[,] { { 12, 1 }, { 23, 12 }, { 13, 23 }, { 1, 13 } };
        //pola koncowe             // endTop      right    bottom      left
        int[,] endFields = new int[,] { { 13, 1 }, { 23, 13 }, { 12, 23 }, { 1, 12 } };

        //intersection fields
        //    x1
        //     A B x2
        // x3  C D
        //        x4
        //private const int[,] crossInner = new int[,] { { 12, 12 }, { 13, 12 }, { 12, 13 }, { 13, 13 } };
        public const int crossInner1X = 12;
        public const int crossInner1Y = 12;
        public const int crossInner2X = 13;
        public const int crossInner2Y = 12;
        public const int crossInner3X = 12;
        public const int crossInner3Y = 13;
        public const int crossInner4X = 13;
        public const int crossInner4Y = 13;
        //x1,x2,x3,x4
        int[,] crossOuter = new int[,] { { 12, 11 }, { 14, 12 }, { 11, 13 }, { 13, 14 } };
        public const int crossOuter1X = 12;//A
        public const int crossOuter1Y = 11;

        public const int crossOuter2X = 14;//B
        public const int crossOuter2Y = 12;

        public const int crossOuter3X = 11;//C
        public const int crossOuter3Y = 13;

        public const int crossOuter4X = 13;//D
        public const int crossOuter4Y = 14;

        bool[,] roadSpace = new bool[25, 25];
        Mutex[,] roadSpaceMutex = new Mutex[25, 25];

        bool[] onlyOne = new bool[4];
        Mutex[] onlyOneMutex = new Mutex[4];

        //storing car characters/id-s
        Queue<string> characters = new Queue<string>();
        Mutex charactersMutex = new Mutex();
        int MAX_CAR_NUMBER = 10;//queue.size;
        private int crossingCarsNumber = 0;
        private Mutex crossingCarsNumberMutex = new Mutex();
        private int[] hungryArray = new int[4];

        public Mutex getCrossingCarsNumberMutex()
        {
            return crossingCarsNumberMutex;
        }

        public Road()
        {
            for (int i = 0; i < 4; i++)
            {
                hungryArray[i] = i;
                onlyOneMutex[i] = new Mutex();
                onlyOne[i] = true;
            }
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
            loadCarCharacters();
        }


        public Mutex getOnlyOneMutex(int fieldId)
        {
            return onlyOneMutex[fieldId];
        }
        public bool getOnlyOne(int fieldId)
        {
            return onlyOne[fieldId];
        }
        public void setOnlyOne(int fieldId, bool value)
        {
            onlyOne[fieldId] = value;
        }

        public bool skipIfBlank(int posX, int posY)
        {
            int i = 0;
            while (true)
            {
                if (hungryArray[i] == 3)
                {
                    GetRoadMutex(crossOuter[i, 0], crossOuter[i, 1]).WaitOne();
                    bool temp = checkSpace(crossOuter[i, 0], crossOuter[i, 1]);
                    GetRoadMutex(crossOuter[i, 0], crossOuter[i, 1]).ReleaseMutex();
                    if (temp == true)
                    {//temp==true car not existing bcs of free space
                        hungryArray[0] = (hungryArray[0] + 1) % 4;
                        hungryArray[1] = (hungryArray[1] + 1) % 4;
                        hungryArray[2] = (hungryArray[2] + 1) % 4;
                        hungryArray[3] = (hungryArray[3] + 1) % 4;
                    }
                    else
                    {
                        //car exists
                        break;
                    }
                }
                i = (i + 1) % 4;
            }
            int carOuter = 0;
            for (int j = 0; j < 4; j++)
            {
                if (crossOuter[j, 0] == posX && crossOuter[j, 1] == posY)
                {
                    carOuter = j;
                }
            }
            if (hungryArray[carOuter] == 3)
            {
                //je??li obecne auto by??o pierwsze w kolejno??ci ->pojedz i ustaw now?? kolejk??
                hungryArray[0] = (hungryArray[0] + 1) % 4;
                hungryArray[1] = (hungryArray[1] + 1) % 4;
                hungryArray[2] = (hungryArray[2] + 1) % 4;
                hungryArray[3] = (hungryArray[3] + 1) % 4;
                //jedz!!!
                return true;
            }
            else
            {
                return false;
            }
        }
        public int getCrossingCarsNumber()
        {
            return crossingCarsNumber;
        }
        public void setCrossingCarsNumber(int vlaue)
        {
            crossingCarsNumber = vlaue;
        }
        public void incCrossingCarsNumber()
        {
            crossingCarsNumber++;
        }
        // public void decCrossingCarsNumber()
        // {
        //     crossingCarsNumber--;
        // }

        public int getQueueLength()
        {
            int q_length = 0;
            charactersMutex.WaitOne();
            q_length = characters.Count;
            charactersMutex.ReleaseMutex();
            return q_length;
        }
        public void pushCharacter(String str)
        {
            charactersMutex.WaitOne();
            characters.Enqueue(str);
            charactersMutex.ReleaseMutex();
        }
        public String popCharacter()
        {
            charactersMutex.WaitOne();
            String returnVal = characters.Dequeue();
            charactersMutex.ReleaseMutex();
            return returnVal;
        }

        private void loadCarCharacters()
        {
            characters.Enqueue("A");
            characters.Enqueue("B");
            characters.Enqueue("C");
            characters.Enqueue("D");
            characters.Enqueue("E");
            characters.Enqueue("F");
            characters.Enqueue("G");
            characters.Enqueue("H");
            characters.Enqueue("I");
            characters.Enqueue("J");
            characters.Enqueue("K");
            characters.Enqueue("L");
            characters.Enqueue("M");
            characters.Enqueue("N");
            characters.Enqueue("O");
            characters.Enqueue("P");
            characters.Enqueue("Q");
            characters.Enqueue("R");
            characters.Enqueue("S");
            characters.Enqueue("T");
            characters.Enqueue("U");
            characters.Enqueue("V");
            characters.Enqueue("W");
            characters.Enqueue("X");
            characters.Enqueue("Y");
            characters.Enqueue("Z");
            MAX_CAR_NUMBER = characters.Count;
        }
        public int getMAX_CAR_NUMBER()
        {
            return MAX_CAR_NUMBER;
        }
        public int getStartPointX(CarPos carPos)
        {
            switch (carPos)
            {
                case CarPos.TOP:
                    return startFields[0, 0];
                case CarPos.RIGHT:
                    return startFields[1, 0];
                case CarPos.BOTTOM:
                    return startFields[2, 0];
                case CarPos.LEFT:
                    return startFields[3, 0];
                default:
                    return 99;
            }
        }
        public int getStartPointY(CarPos carPos)
        {
            switch (carPos)
            {
                case CarPos.TOP:
                    return startFields[0, 1];
                case CarPos.RIGHT:
                    return startFields[1, 1];
                case CarPos.BOTTOM:
                    return startFields[2, 1];
                case CarPos.LEFT:
                    return startFields[3, 1];
                default:
                    return 99;
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
                if (pos_x == crossOuter1X && pos_y == crossOuter1Y)
                {
                    return false;
                }
                if (pos_x == crossOuter2X && pos_y == crossOuter2Y)
                {
                    return false;
                }
                if (pos_x == crossOuter3X && pos_y == crossOuter3Y)
                {
                    return false;
                }
                if (pos_x == crossOuter4X && pos_y == crossOuter4Y)
                {
                    return false;
                }
                // if (pos_x == crossOuter[i, 0] && pos_y == crossOuter[i, 1])
                // {
                //     return false;
                // }
            }
            return true;
        }

        public bool crossingEntrance(int pos_x, int pos_y)
        {
            for (int i = 0; i < 4; i++)
            {
                if (pos_x == crossOuter[i, 0] && pos_y == crossOuter[i, 1])
                {
                    return true;
                }
            }
            return false;
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