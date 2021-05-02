using System;
using System.Threading;
using Terminal.Gui;


namespace crossing1

{
    public class Program
    {
        static private bool programRunning = true;
        Mutex prun = new Mutex();
        Car myCar;
        Road simRoad;
        static void Main(string[] args)
        {
            Program program = new Program();
            program.ProgramGUI();
            //program.notStatic();
        }
        public bool getPrun()
        {
            prun.WaitOne();
            bool temp = programRunning;
            prun.ReleaseMutex();
            return temp;
        }

        private void StartThreads()
        {
            // for(int i=0 i<carNumber;i++){
            //     cars[i]= new Car();
            //     Thread newCarThread = new Thread(new ThreadStart(cars[i].ThreadProc));
            //     newCarThread.Name = String.Format("{0}", i);
            //     newCarThread.Start();
            //     carThreads[i].setThread(newCarThread);
            // }
            myCar = new Car(CarPos.TOP, CarDirection.FORWARD, 'G', this, simRoad);
            Thread newCarThread = new Thread(new ThreadStart(myCar.ThreadProc));
            newCarThread.Start();
            myCar.setThread(newCarThread);
        }

        public void ProgramGUI()
        {
            simRoad = new Road();
            StartThreads();
            Application.Init();
            var top = Application.Top;
            // Creates a menubar f9
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_F9 Menu", new MenuItem [] {
                    new MenuItem ("_Quit", "", () => { top.Running = false; }),
                    new MenuItem ("_Stopthreads", "", () => { programRunning = false; }),
                }),
            });
            top.Add(menu);
            //Simulation window
            var win = new Window("Crossing Problem") { X = 0, Y = 1, Width = Dim.Fill(), Height = Dim.Fill() };
            //dodanie poszczegolnych elementow
            addBoundaries(win);

            var testLabel = new Label(myCar.getGraphic())
            {
                X = myCar.getPosX(),
                Y = myCar.getPosY()
            };
            win.Add(testLabel);
            //Main loop, update data here
            bool timer(MainLoop caller)
            {
                //get dangerous data from???               
                //updateInfo(0, pb_0, philosopherStatus_0);
                // updateForks(0, forkLabel_0);
                //powinno byc zabezpieczone!
                testLabel.X = myCar.getPosX();
                testLabel.Y = myCar.getPosY();
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
