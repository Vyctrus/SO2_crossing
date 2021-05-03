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

        //pozycja którą chce następnie zająć auto
        private int next_pos_X = 0;
        private int next_pos_Y = 0;
        crossing1.Program program;
        crossing1.Road road;
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
        public Car(CarPos carPos, CarDirection carDirection, String carGraphic, Program program1, Road carRoad)
        {
            program = program1;
            road = carRoad;

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
                if (checkIfEnd())
                {
                    //uwolnij pozycje
                    carExistsMutex.WaitOne();
                    carExists = false;
                    carExistsMutex.ReleaseMutex();
                    break;
                }
                lockPosition();
                bool temp = tryToMove();
                unlockPosition();
                if (!temp) { break; }
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
                    next_pos_Y = pos_Y--;
                    break;
                case CarRotation.RIGHT:
                    next_pos_X = pos_X++;
                    next_pos_Y = pos_Y;
                    break;
                case CarRotation.DOWN:
                    next_pos_X = pos_X;
                    next_pos_Y = pos_Y++;
                    break;
                case CarRotation.LEFT:
                    next_pos_X = pos_X--;
                    next_pos_Y = pos_Y;
                    break;
            }
        }
        bool tryToMove()
        {
            //wykonaj ruch jeżeli możesz
            //canmove - jeżeli wolna przestrzeń i nie obsługujesz skrzyżowania
            if (road.straightRoad(pos_X, pos_Y))
            {
                step();
            }
            else
            {
                return false;
                //obsługa skrzyżowania
            }
            return true;
            //obsługa skrzyżowania

            // while (!moveFinished)
            // {
            // step();
            //   Thread.Sleep(speed);
            // if (road.checkSpace(next_pos_X, next_pos_Y) == false && checkCrossingRules())
            // {
            //     //kod jeśli można jechać-> jedź
            //     road.reserveSpace(next_pos_X, next_pos_Y);
            //     road.freeSpace(pos_X, pos_Y);
            //     pos_X = next_pos_X;
            //     pos_Y = next_pos_Y;
            //     moveFinished = true;
            // }
            // else
            // {
            //     Thread.Sleep(speed);
            // }
            //}
        }


    }
}
