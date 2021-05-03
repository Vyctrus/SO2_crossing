using System;
using System.Collections.Generic;
using System.Threading;
using Terminal.Gui;


namespace crossing1

{
    public class Program
    {
        //static private bool programRunning = true;
        static private bool safeRunning1 = true;
        Mutex prun = new Mutex();
        List<Car> cars = new List<Car>();
        //Car myCar;
        Road simRoad;
        static Random rand = new Random();
        static Object randLock = new Object();
        static void Main(string[] args)
        {
            Program program = new Program();
            program.ProgramGUI();
            //program.notStatic();
        }

        public bool getPrun()
        {
            prun.WaitOne();
            bool temp = safeRunning1;
            prun.ReleaseMutex();
            return temp;
        }
        public void setPrun(bool value)
        {
            prun.WaitOne();
            safeRunning1 = value;
            prun.ReleaseMutex();
        }


        private void GenerateCarsProc()
        {
            int MAX_CAR_NUMBER = simRoad.getMAX_CAR_NUMBER();
            int CURRENT_CAR_NUMBER = 0;
            int frequency = 1000;
            int genX = 0;
            int genY = 0;
            CarPos newCarPos = CarPos.TOP;
            CarDirection newCarDir = CarDirection.FORWARD;
            while (getPrun())
            {
                if (CURRENT_CAR_NUMBER < MAX_CAR_NUMBER)
                {
                    //losuj carPos
                    int temp = 0;
                    lock (randLock)
                    {
                        temp = rand.Next() % 4;
                    }
                    switch (temp)
                    {
                        case 0:
                            newCarPos = CarPos.TOP;
                            break;
                        case 1:
                            newCarPos = CarPos.RIGHT;
                            break;
                        case 2:
                            newCarPos = CarPos.BOTTOM;
                            break;
                        case 3:
                            newCarPos = CarPos.LEFT;
                            break;
                    }
                    genX = simRoad.getStartPointX(newCarPos);
                    genY = simRoad.getStartPointY(newCarPos);
                    simRoad.GetRoadMutex(genX, genY).WaitOne();
                    // :::::::::::
                    if (simRoad.checkSpace(genX, genY))
                    {
                        lock (randLock)
                        {
                            temp = rand.Next() % 3;
                        }
                        switch (temp)
                        {
                            case 0:
                                newCarDir = CarDirection.FORWARD;
                                break;
                            case 1:
                                newCarDir = CarDirection.TURN_LEFT;
                                break;
                            case 2:
                                newCarDir = CarDirection.TURN_RIGHT;
                                break;
                        }

                        Car newCar = new Car(newCarPos, newCarDir, simRoad.popCharacter(), this, simRoad);
                        Thread newCarThread = new Thread(new ThreadStart(newCar.ThreadProc));
                        //newCarThread.Name= String.Format("{0}", i);
                        newCarThread.Start();
                        newCar.setThread(newCarThread);
                        cars.Add(newCar);
                        CURRENT_CAR_NUMBER++;
                    }
                    simRoad.GetRoadMutex(genX, genY).ReleaseMutex();
                }
                Thread.Sleep(frequency);
            }
        }

        public void ProgramGUI()
        {
            simRoad = new Road();
            //StartThreads();
            Thread generator = new Thread(new ThreadStart(GenerateCarsProc));
            generator.Start();
            Application.Init();
            var top = Application.Top;
            // Creates a menubar f9
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_F9 Menu", new MenuItem [] {
                    new MenuItem ("_Quit", "", () => { top.Running = false; }),
                    new MenuItem ("_Stopthreads", "", () => { setPrun(false); }),
                }),
            });
            top.Add(menu);
            //Simulation window
            var win = new Window("Crossing Problem") { X = 0, Y = 1, Width = Dim.Fill(), Height = Dim.Fill() };
            //dodanie poszczegolnych elementow
            addBoundaries(win);

            // var testLabel = new Label(myCar.getGraphic())
            // {
            //     X = myCar.getPosX(),
            //     Y = myCar.getPosY()
            // };
            //win.Add(testLabel);

            //Main loop, update data here
            bool timer(MainLoop caller)
            {
                win.RemoveAll();
                addBoundaries(win);
                //get dangerous data from???               
                //updateInfo(0, pb_0, philosopherStatus_0);
                // updateForks(0, forkLabel_0);
                //powinno byc zabezpieczone!

                //remove not existing/dead cars
                foreach (Car singleCar in cars.ToArray())
                {
                    if (singleCar.checkIfCarExists() == false)
                    {
                        simRoad.pushCharacter(singleCar.getGraphic());
                        cars.Remove(singleCar);
                    }
                }

                //print existing ones
                foreach (Car singleCar in cars.ToArray())
                {
                    var singleCarLabel = new Label(singleCar.getGraphic())
                    {
                        X = singleCar.getPosX(),
                        Y = singleCar.getPosY()
                    };
                    win.Add(singleCarLabel);
                }

                //update graphics + generate new cars?
                return true;
            }
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(200), timer);
            top.Add(win);
            Application.Run();
        }

        private void addBoundaries(Window win)
        {
            for (int i = 1; i < 24; i++)
            {
                if (i == 12 || i == 13)
                {
                    continue;
                }
                var boundaries = new Label("%")
                {
                    X = i,
                    Y = 11
                };
                win.Add(boundaries);
                var boundaries2 = new Label("%")
                {
                    X = i,
                    Y = 14
                };
                win.Add(boundaries2);
                var boundaries3 = new Label("%")
                {
                    X = 11,
                    Y = i
                };
                win.Add(boundaries3);
                var boundaries4 = new Label("%")
                {
                    X = 14,
                    Y = i
                };
                win.Add(boundaries4);

            }

        }
    }
}
