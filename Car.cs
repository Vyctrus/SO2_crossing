using System;
using System.Threading;
using Terminal.Gui;
using crossing1;

namespace crossing1
{
    enum CarPos
    {
        TOP,
        RIGHT,
        BOTTOM,
        LEFT
    }

    enum CarDirection
    {
        FORWARD,
        TURN_RIGHT,
        TURN_LEFT
    }
    enum CarRotation
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    class Car
    {
        Thread threadAction;
        private bool carExists = true;
        private Mutex carExistsMutex = new Mutex();
        private int pos_X = 0;
        private int pos_Y = 0;
        private int posOrigin_X = 0;
        private int posOrigin_Y = 0;
        private Mutex positionMutex;
        private int speed = 500; //?wspieranie predkosci?
        private static Random randSpeed = new Random();

        private String graphic = "A";
        private Terminal.Gui.Attribute carColor;//= new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black);
        private CarRotation carRot; // :D
        private CarDirection carDir;
        private CarPos carDestination;

        //pozycja którą chce następnie zająć auto
        private int next_pos_X = 0;
        private int next_pos_Y = 0;
        private int old_pos_X = 0;
        private int old_pos_Y = 0;
        private bool shouldFree = false;
        crossing1.Program program;
        crossing1.Road road;
        SemaphoreSlim roadGod;

        private int rightPosX = 999;
        private int rightPosY = 999;
        private int crossingPos = 999;

        //i dont wanna to use road in retriving constants
        public const int crossOuter1X = 12;//A
        public const int crossOuter1Y = 11;

        public const int crossOuter2X = 14;//B
        public const int crossOuter2Y = 12;

        public const int crossOuter3X = 11;//C
        public const int crossOuter3Y = 13;

        public const int crossOuter4X = 13;//D
        public const int crossOuter4Y = 14;
        private const bool hierarchy = true;

        private bool roadWithoutPrio()
        {
            if (hierarchy)
            {
                //A has prio top
                if (pos_X == crossOuter1X && pos_Y == crossOuter1Y)
                {
                    return false;
                }
                //B right
                if (pos_X == crossOuter2X && pos_Y == crossOuter2Y)
                {
                    return true;
                }
                //C left
                if (pos_X == crossOuter3X && pos_Y == crossOuter3Y)
                {
                    return true;
                }
                //D bottom
                if (pos_X == crossOuter4X && pos_Y == crossOuter4Y)
                {
                    return true;
                }
            }
            return true;
        }

        private bool getCarDecision()
        {
            if (randSpeed.Next(100) < randSpeed.Next(10, 70))
            {
                return true;
            }
            return false;
        }

        public Terminal.Gui.Attribute getCarColor()
        {
            return carColor;
        }
        public void setThread(Thread passed)
        {
            threadAction = passed;
        }
        public Thread getThread()
        {
            return threadAction;
        }
        public String getGraphic()
        {
            return graphic.ToString();
        }
        //use only with lock&unlock functionS!
        public int getPosX()
        {
            return pos_X;
        }
        public int getPosY()
        {
            return pos_Y;
        }
        public void lockPosition()
        {
            positionMutex.WaitOne();
        }
        public void unlockPosition()
        {
            positionMutex.ReleaseMutex();
        }
        public Car(CarPos carPos, CarDirection carDirection, String carGraphic, Program program1, Road carRoad, SemaphoreSlim godOfRoad)
        {
            program = program1;
            road = carRoad;
            roadGod = godOfRoad;
            speed = randSpeed.Next(100, 500);
            carDir = carDirection;
            switch (carDirection)
            {
                case CarDirection.FORWARD:
                    switch (carPos)
                    {
                        case CarPos.TOP:
                            carDestination = CarPos.BOTTOM;
                            break;
                        case CarPos.RIGHT:
                            carDestination = CarPos.LEFT;
                            break;
                        case CarPos.BOTTOM:
                            carDestination = CarPos.TOP;
                            break;
                        case CarPos.LEFT:
                            carDestination = CarPos.RIGHT;
                            break;
                    }
                    break;
                case CarDirection.TURN_RIGHT:
                    switch (carPos)
                    {
                        case CarPos.TOP:
                            carDestination = CarPos.LEFT;
                            break;
                        case CarPos.RIGHT:
                            carDestination = CarPos.TOP;
                            break;
                        case CarPos.BOTTOM:
                            carDestination = CarPos.RIGHT;
                            break;
                        case CarPos.LEFT:
                            carDestination = CarPos.BOTTOM;
                            break;
                    }
                    break;
                case CarDirection.TURN_LEFT:
                    switch (carPos)
                    {
                        case CarPos.TOP:
                            carDestination = CarPos.RIGHT;
                            break;
                        case CarPos.RIGHT:
                            carDestination = CarPos.BOTTOM;
                            break;
                        case CarPos.BOTTOM:
                            carDestination = CarPos.LEFT;
                            break;
                        case CarPos.LEFT:
                            carDestination = CarPos.TOP;
                            break;
                    }
                    break;
            }

            switch (carDestination)
            {
                case CarPos.TOP:
                    carColor = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black);
                    break;
                case CarPos.RIGHT:
                    carColor = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black);
                    break;
                case CarPos.BOTTOM:
                    carColor = new Terminal.Gui.Attribute(Color.BrightRed, Color.Black);
                    break;
                case CarPos.LEFT:
                    carColor = new Terminal.Gui.Attribute(Color.BrightBlue, Color.Black);
                    break;
            }
            //pozniej zalezne od kierunku
            graphic = carGraphic;
            positionMutex = new Mutex();
            lockPosition();
            pos_X = road.getStartPointX(carPos);
            pos_Y = road.getStartPointY(carPos);
            posOrigin_X = pos_X;
            posOrigin_Y = pos_Y;
            unlockPosition();
            switch (carPos)
            {
                case CarPos.TOP:
                    carRot = CarRotation.DOWN;
                    break;
                case CarPos.RIGHT:
                    carRot = CarRotation.LEFT;
                    break;
                case CarPos.BOTTOM:
                    carRot = CarRotation.UP;
                    break;
                case CarPos.LEFT:
                    carRot = CarRotation.RIGHT;
                    break;
            }
        }
        public bool checkIfCarExists()
        {
            bool safeCarExists = true;
            carExistsMutex.WaitOne();
            safeCarExists = carExists;
            carExistsMutex.ReleaseMutex();
            return safeCarExists;
        }
        public void ThreadProc()
        {
            while (program.getPrun())
            {
                Thread.Sleep(speed);
                lockPosition();
                bool temp = tryToMove();
                unlockPosition();
                if (!temp)
                {
                    carExistsMutex.WaitOne();
                    carExists = false;
                    carExistsMutex.ReleaseMutex();
                    break;
                }
            }
        }


        //jak zawracaja: w oparciu o miejsce gdzie są i w oparciu o swoje destination 
        //licz nową pozycja w zależnośći od tego gdzie auto jedzie
        //po dotarciu do skrzyrzowania na kazdym polu sprawdz kierunek
        //po wykonaniu ruchu update pozycji next_pos
        void step()
        {
            switch (carRot)
            {
                case CarRotation.UP:
                    next_pos_X = pos_X;
                    next_pos_Y = pos_Y - 1;//pos_Y--;
                    break;
                case CarRotation.RIGHT:
                    next_pos_X = pos_X + 1;//pos_X++;
                    next_pos_Y = pos_Y;
                    break;
                case CarRotation.DOWN:
                    next_pos_X = pos_X;
                    next_pos_Y = pos_Y + 1;//pos_Y++;
                    break;
                case CarRotation.LEFT:
                    next_pos_X = pos_X - 1;//pos_X--;
                    next_pos_Y = pos_Y;
                    break;
            }
        }
        void unlockYourLane(Road road)
        {
            if (posOrigin_X == 12 && posOrigin_Y == 1)
            {//top
                road.getOnlyOneMutex(0).WaitOne();
                road.setOnlyOne(0, true);
                road.getOnlyOneMutex(0).ReleaseMutex();
            }
            if (posOrigin_X == 23 && posOrigin_Y == 12)
            {//right
                road.getOnlyOneMutex(1).WaitOne();
                road.setOnlyOne(1, true);
                road.getOnlyOneMutex(1).ReleaseMutex();
            }
            if (posOrigin_X == 13 && posOrigin_Y == 23)
            {//bottom
                road.getOnlyOneMutex(3).WaitOne();
                road.setOnlyOne(3, true);
                road.getOnlyOneMutex(3).ReleaseMutex();
            }
            if (posOrigin_X == 1 && posOrigin_Y == 13)
            {//left
                road.getOnlyOneMutex(2).WaitOne();
                road.setOnlyOne(2, true);
                road.getOnlyOneMutex(2).ReleaseMutex();
            }
        }
        bool singleMove()
        {
            step();
            road.GetRoadMutex(next_pos_X, next_pos_Y).WaitOne();
            if (road.checkSpace(next_pos_X, next_pos_Y))
            {
                //you can go into next_pos
                //zajmij miejsce ,zmien miejsce, zwolnij stare miejsce
                road.setSpaceOccupied(next_pos_X, next_pos_Y);
                old_pos_X = pos_X;
                old_pos_Y = pos_Y;
                pos_X = next_pos_X;
                pos_Y = next_pos_Y;
                shouldFree = true;
            }
            else
            {
                shouldFree = false;
            }
            road.GetRoadMutex(next_pos_X, next_pos_Y).ReleaseMutex();
            //avoiding nested mutex lock!!!            
            if (shouldFree)
            {//
                road.GetRoadMutex(old_pos_X, old_pos_Y).WaitOne();
                road.setSpaceFree(old_pos_X, old_pos_Y);
                road.GetRoadMutex(old_pos_X, old_pos_Y).ReleaseMutex();
                if (pos_X == 13 && pos_Y == 11)
                {
                    unlockYourLane(road);
                }
                if (pos_X == 14 && pos_Y == 13)
                {
                    unlockYourLane(road);
                }
                if (pos_X == 11 && pos_Y == 12)
                {
                    unlockYourLane(road);
                }
                if (pos_X == 12 && pos_Y == 14)
                {
                    unlockYourLane(road);
                }
                return true;

            }
            return false;
        }

        void correctRotation(int pos_X, int pos_Y, CarPos carDestination)
        {
            switch (carDestination)
            {
                case CarPos.TOP:
                    if (pos_X == 12 && pos_Y == 12)
                    {
                        //niemozliwe
                    }
                    if (pos_X == 13 && pos_Y == 12)
                    {
                        carRot = CarRotation.UP;
                    }
                    if (pos_X == 12 && pos_Y == 13)
                    {
                        carRot = CarRotation.RIGHT;
                    }
                    if (pos_X == 13 && pos_Y == 13)
                    {
                        carRot = CarRotation.UP;
                    }
                    break;
                case CarPos.RIGHT:
                    if (pos_X == 12 && pos_Y == 12)
                    {
                        carRot = CarRotation.DOWN;
                    }
                    if (pos_X == 13 && pos_Y == 12)
                    {
                        //niemozliwe
                    }
                    if (pos_X == 12 && pos_Y == 13)
                    {
                        carRot = CarRotation.RIGHT;
                    }
                    if (pos_X == 13 && pos_Y == 13)
                    {
                        carRot = CarRotation.RIGHT;
                    }
                    break;
                case CarPos.LEFT:
                    if (pos_X == 12 && pos_Y == 12)
                    {
                        carRot = CarRotation.LEFT;
                    }
                    if (pos_X == 13 && pos_Y == 12)
                    {
                        carRot = CarRotation.LEFT;
                    }
                    if (pos_X == 12 && pos_Y == 13)
                    {
                        //niemozliwe
                    }
                    if (pos_X == 13 && pos_Y == 13)
                    {
                        carRot = CarRotation.UP;
                    }
                    break;

                case CarPos.BOTTOM:
                    if (pos_X == 12 && pos_Y == 12)
                    {
                        carRot = CarRotation.DOWN;
                    }
                    if (pos_X == 13 && pos_Y == 12)
                    {
                        carRot = CarRotation.LEFT;
                    }
                    if (pos_X == 12 && pos_Y == 13)
                    {
                        carRot = CarRotation.DOWN;
                    }
                    if (pos_X == 13 && pos_Y == 13)
                    {
                        //niemożliwe
                    }
                    break;
            }
        }
        bool endOfRoad(int pos_X, int pos_Y, Road road)
        {
            if (pos_X >= 24 || pos_Y >= 24 || pos_X <= 0 || pos_Y <= 0)
            {
                road.GetRoadMutex(pos_X, pos_Y).WaitOne();
                road.setSpaceFree(pos_X, pos_Y);
                road.GetRoadMutex(pos_X, pos_Y).ReleaseMutex();
                return true;
            }
            return false;
        }

        bool tryToMove()
        {
            if (endOfRoad(pos_X, pos_Y, road))
            {
                return false;
            }

            //wykonaj ruch jeżeli możesz
            if (road.straightRoad(pos_X, pos_Y))
            {
                singleMove();
            }
            else
            {   //crossing rules
                roadGod.Wait();
                getRightCarPositions(pos_X, pos_Y);

                bool lanePassPermission = false;
                road.getOnlyOneMutex(crossingPos).WaitOne();
                lanePassPermission = road.getOnlyOne(crossingPos);
                road.getOnlyOneMutex(crossingPos).ReleaseMutex();

                if (lanePassPermission)
                {
                    if (carDir == CarDirection.TURN_RIGHT)
                    {
                        //singleMove();
                        if (singleMove())
                        {
                            road.getOnlyOneMutex(crossingPos).WaitOne();
                            lanePassPermission = road.getOnlyOne(crossingPos);
                            road.setOnlyOne(crossingPos, false);
                            road.getOnlyOneMutex(crossingPos).ReleaseMutex();
                        }
                        //..^-..ZABLOKUJ PAS
                    }
                    else
                    {
                        //otworz mutex sasiada i zamknij go dopiero po rozpatrzeniu calej skewencji ruchu
                        //    road.GetRoadMutex(rightPosX, rightPosY).WaitOne();
                        //
                        road.GetRoadMutex(12, 12).WaitOne();
                        road.GetRoadMutex(13, 12).WaitOne();
                        road.GetRoadMutex(12, 13).WaitOne();
                        road.GetRoadMutex(13, 13).WaitOne();

                        //kolizje hipotetyczne
                        if (!roadWithoutPrio())
                        {
                            //przy wykrywaniu kolizji powinien byc tryb z tylko jedna droga pierwszenstwa
                            //dwie dorgi z pierwszeństwem poskutkowałyby podwójnym losowaniem czy zaszła kolizja
                            int ccNumber2 = 0;
                            if (!road.checkSpace(12, 12) || getCarDecision())
                            {
                                ccNumber2++;
                            }
                            if (!road.checkSpace(13, 12) || getCarDecision())
                            {
                                ccNumber2++;
                            }
                            if (!road.checkSpace(12, 13) || getCarDecision())
                            {
                                ccNumber2++;
                            }
                            if (!road.checkSpace(13, 13) || getCarDecision())
                            {
                                ccNumber2++;
                            }
                            if (ccNumber2 == 4)
                            {
                                road.getCrossingCarsNumberMutex().WaitOne();//bad name but already existed
                                road.incCrossingCarsNumber();
                                road.getCrossingCarsNumberMutex().ReleaseMutex();
                            }
                            //co z przypadkiem keidy choc jedno auto skreca w prawo?
                            //komu to sprawdzac- wszelkim autom na skrzyżowaniu?
                        }

                        if ((!road.checkSpace(rightPosX, rightPosY)) && roadWithoutPrio())
                        {
                            //warunek- jeśli po twojej prawej jest false, to znaczy ze ktos tam stoi
                            //hierarchia zasobów- zignoruj to że ktoś stoi bo pas na którym jesteś ma pierwszeństwo

                            //wait for your turn
                        }
                        else
                        {
                            //youCanTryDrive();
                            int ccNumber = 0;
                            if (!road.checkSpace(12, 12))
                            {
                                ccNumber++;
                            }
                            if (!road.checkSpace(13, 12))
                            {
                                ccNumber++;
                            }
                            if (!road.checkSpace(12, 13))
                            {
                                ccNumber++;
                            }
                            if (!road.checkSpace(13, 13))
                            {
                                ccNumber++;
                            }

                            //!!!!!!!!!...w tym miejscu trzeba trzymac zalokowany mutex drogi na którą się wybiera nasz auto
                            //puszczamy dopiero po skonczeniu ruchu auta
                            if (ccNumber >= 3)
                            {
                                //u cant drive
                            }
                            else
                            {
                                //u can drive
                                singleMove();
                            }
                        }
                        road.GetRoadMutex(13, 13).ReleaseMutex();
                        road.GetRoadMutex(12, 13).ReleaseMutex();
                        road.GetRoadMutex(13, 12).ReleaseMutex();
                        road.GetRoadMutex(12, 12).ReleaseMutex();
                        //roadGod.Release();
                    }
                }
                else
                {
                    //wait for your turn
                }


                if (false)
                {//rodGod version
                 //old code
                    roadGod.Wait();//roadGod is listening one car at time
                    int ccNumber = 0;
                    road.GetRoadMutex(12, 12).WaitOne();
                    if (!road.checkSpace(12, 12))
                    {
                        ccNumber++;
                    }
                    road.GetRoadMutex(12, 12).ReleaseMutex();

                    road.GetRoadMutex(13, 12).WaitOne();
                    if (!road.checkSpace(13, 12))
                    {
                        ccNumber++;
                    }
                    road.GetRoadMutex(13, 12).ReleaseMutex();

                    road.GetRoadMutex(12, 13).WaitOne();
                    if (!road.checkSpace(12, 13))
                    {
                        ccNumber++;
                    }
                    road.GetRoadMutex(12, 13).ReleaseMutex();

                    road.GetRoadMutex(13, 13).WaitOne();
                    if (!road.checkSpace(13, 13))
                    {
                        ccNumber++;
                    }
                    road.GetRoadMutex(13, 13).ReleaseMutex();

                    road.getCrossingCarsNumberMutex().WaitOne();
                    road.setCrossingCarsNumber(ccNumber);
                    road.getCrossingCarsNumberMutex().ReleaseMutex();

                    if (ccNumber < 3)
                    {
                        //możesz probowac wpuscic auto
                        //zdefiniuj pole jest puste: road.CheckSpace()==true
                        //sprawdzaj po kolejnych crossOuter
                        //skip queue until car exists
                        bool carCanGo = road.skipIfBlank(pos_X, pos_Y);
                        if (carCanGo)
                        {
                            singleMove();
                            // road.getCrossingCarsNumberMutex().WaitOne();
                            // road.incCrossingCarsNumber();
                            // road.getCrossingCarsNumberMutex().ReleaseMutex();
                        }
                    }
                    road.getCrossingCarsNumberMutex().ReleaseMutex();
                    //roadGod.Release();
                }

                roadGod.Release();
            }
            //car after trying to move
            correctRotation(pos_X, pos_Y, carDestination);
            return true;
        }
        void getRightCarPositions(int pos_X, int pos_Y)
        {
            //  A
            //  X X B
            //C X X 
            //    D
            //A
            if (pos_X == crossOuter1X && pos_Y == crossOuter1Y)
            {
                rightPosX = crossOuter3X;//C
                rightPosY = crossOuter3Y;
                crossingPos = 0;
            }
            //B
            if (pos_X == crossOuter2X && pos_Y == crossOuter2Y)
            {
                rightPosX = crossOuter1X;//A
                rightPosY = crossOuter1Y;
                crossingPos = 1;
            }
            //C
            if (pos_X == crossOuter3X && pos_Y == crossOuter3Y)
            {
                rightPosX = crossOuter4X;
                rightPosY = crossOuter4Y;
                crossingPos = 2;
            }
            //D
            if (pos_X == crossOuter4X && pos_Y == crossOuter4Y)
            {
                rightPosX = crossOuter2X;
                rightPosY = crossOuter2Y;
                crossingPos = 3;
            }

        }
    }
}
