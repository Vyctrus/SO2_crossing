using System;
using System.Threading;
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
        private Mutex positionMutex;
        private int speed = 500; //?wspieranie predkosci?

        private int end_pos_X = 30;
        private int end_pos_Y = 0;

        private String graphic = "A";
        private CarRotation carRot; // :D
        //private CarDirection carDir;
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

            //pozniej zalezne od kierunku
            graphic = carGraphic;
            positionMutex = new Mutex();
            //
            lockPosition();
            //carPos = CarPos.TOP;
            pos_X = road.getStartPointX(carPos);
            pos_Y = road.getStartPointY(carPos);

            unlockPosition();
            //ustawienie poczatkowej rotacji
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
            //test
            //carRot = CarRotation.DOWN;
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
                // if (checkIfEnd())
                // {
                //     //uwolnij pozycje
                //     carExistsMutex.WaitOne();
                //     carExists = false;
                //     carExistsMutex.ReleaseMutex();
                //     break;
                // }
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
        public bool checkIfEnd()
        {
            bool returnVal = false;
            lockPosition();
            if (pos_X == end_pos_X && pos_Y == end_pos_Y)
            {
                returnVal = true;
            }
            unlockPosition();
            return returnVal;
        }

        //jak zawracaja? w oparciu o miejsce gdzie są?
        //licz nową pozycja w zależnośći od tego gdzie auto jedzie
        //po dotarciu do skrzyrzowania na kazdym polu sprawdz kierunek
        //ustaw carRotationw oparciu o kierunek, w oparciu o rotation
        //wykonuj ruchy
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
        void singleMove()
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
                // if (pos_X == 13 && pos_Y == 11)
                // {
                //     road.getCrossingCarsNumberMutex().WaitOne();
                //     road.decCrossingCarsNumber();
                //     road.getCrossingCarsNumberMutex().ReleaseMutex();
                // }
                // if (pos_X == 14 && pos_Y == 13)
                // {
                //     road.getCrossingCarsNumberMutex().WaitOne();
                //     road.decCrossingCarsNumber();
                //     road.getCrossingCarsNumberMutex().ReleaseMutex();
                // }
                // if (pos_X == 11 && pos_Y == 12)
                // {
                //     road.getCrossingCarsNumberMutex().WaitOne();
                //     road.decCrossingCarsNumber();
                //     road.getCrossingCarsNumberMutex().ReleaseMutex();
                // }
                // if (pos_X == 12 && pos_Y == 14)
                // {
                //     road.getCrossingCarsNumberMutex().WaitOne();
                //     road.decCrossingCarsNumber();
                //     road.getCrossingCarsNumberMutex().ReleaseMutex();
                // }
            }
        }
        bool tryToMove()
        {
            if (pos_X >= 24 || pos_Y >= 24 || pos_X <= 0 || pos_Y <= 0)
            {
                road.GetRoadMutex(pos_X, pos_Y).WaitOne();
                road.setSpaceFree(pos_X, pos_Y);
                road.GetRoadMutex(pos_X, pos_Y).ReleaseMutex();
                return false;
                //sprawdz czy auto wyjechalo poza obszar wtedy return false
                //obroc auto jeśli jest na skrzyżowaniu w oparciu o destination
            }
            //wykonaj ruch jeżeli możesz
            //canmove - jeżeli wolna przestrzeń i nie obsługujesz skrzyżowania
            if (road.straightRoad(pos_X, pos_Y))
            {
                singleMove();
            }
            else
            {   //crossing rules
                // if (road.crossingEntrance(pos_X, pos_Y)) domyslnie obsluga tylko wejsc
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
                        //go
                        singleMove();
                        // road.getCrossingCarsNumberMutex().WaitOne();
                        // road.incCrossingCarsNumber();
                        // road.getCrossingCarsNumberMutex().ReleaseMutex();
                    }
                }
                //road.getCrossingCarsNumberMutex().ReleaseMutex();
                roadGod.Release();

            }
            // if (pos_X >= 24 || pos_Y >= 24 || pos_X <= 0 || pos_X <= 0)
            // {
            //     road.GetRoadMutex(pos_X, pos_Y).WaitOne();
            //     road.setSpaceFree(pos_X, pos_Y);
            //     road.GetRoadMutex(pos_X, pos_Y).ReleaseMutex();
            //     return false;
            //     //sprawdz czy auto wyjechalo poza obszar wtedy return false
            //     //obroc auto jeśli jest na skrzyżowaniu w oparciu o destination
            // }

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
            return true;

        }

    }
}
