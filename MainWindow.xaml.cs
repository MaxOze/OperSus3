using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace _3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Mutex mutex = new Mutex();
        private ObservableCollection<Process> processesList;
        private bool run = true;
        private Process executedProcess = null;
        private Func<Process, Process, int> sorting = null;
        public MainWindow()
        {
            this.DataContext = this;
            processesList = new ObservableCollection<Process>();

            Thread executingThread = new Thread(() => ExecuteProcess());
            Thread showThread = new Thread(() => ShowExecutedProcess());
            Thread timeThread = new Thread(() => TimeChange());
            InitializeComponent();
            ProcessList.ItemsSource = processesList;

            sorting = Process.sortPlan;

            executingThread.Start();
            showThread.Start();
            timeThread.Start();

        }

        private void AddProcessButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int CP = Convert.ToInt32(AllocatedCPText.Text);
                int memory = Convert.ToInt32(AllocatedMemoryText.Text);
                Process process = new Process(CP, memory);
                processesList.Add(process);
                sortAfterAdd(sorting);
            }
            catch
            {

            }

            AllocatedCPText.Text = "";
            AllocatedMemoryText.Text = "";
        }

        private void sortAfterAdd(Func<Process, Process, int> sorting)
        {
            if (processesList.Count > 1)
            {
                mutex.WaitOne();


                bool isSorted;

                do
                {
                    isSorted = true;

                    for (var i = 0; i < processesList.Count - 1; i++)
                    {
                        if (sorting(processesList[i], processesList[i + 1]) > 0)
                        {
                            (processesList[i], processesList[i + 1]) = (processesList[i + 1], processesList[i]);
                            isSorted = false;
                        }
                    }
                } while (isSorted != true);


                mutex.ReleaseMutex();
            }
        }

        private void ExecuteProcess()
        {
            while (true)
            {
                if (run)
                {
                    if (processesList.Count != 0 || executedProcess != null)
                    {
                        if (executedProcess != null)
                        {
                            executedProcess.executeTime--;
                            if (executedProcess.executeTime == 0)
                                executedProcess = null;
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            mutex.WaitOne();
                            executedProcess = processesList.First();
                            Dispatcher.Invoke(() => processesList.RemoveAt(0));
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
        }

        private void ShowExecutedProcess()
        {
            while (true)
            {
                mutex.WaitOne();
                if (executedProcess != null)
                {
                    Dispatcher.Invoke(() => CurrentProcessText.Text = executedProcess.ToString());
                }
                else
                {
                    Dispatcher.Invoke(() => CurrentProcessText.Text = "Ожидание");
                }
                mutex.ReleaseMutex();
                Thread.Sleep(500);
            }
        }

        private void TimeChange()
        {
            while (true)
            {
                Dispatcher.Invoke(() => TimeBlock.Text = DateTime.Now.ToLongTimeString());
                if (Dispatcher.Invoke(() => processesList.Count > 0))
                {
                    mutex.WaitOne();
                    foreach (var process in processesList)
                    {
                        process.waitingTime++;
                    }
                    Dispatcher.Invoke(() => ProcessList.Items.Refresh());
                    mutex.ReleaseMutex();
                }

                if (Dispatcher.Invoke(() => ButtonHRN.IsChecked == true))
                    Dispatcher.Invoke(() => sortAfterAdd(sorting));

                Thread.Sleep(1000);
            }
        }

        private void Alghoritm_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton pressed = (RadioButton)sender;
            if (pressed.Content.Equals("Plan"))
                sorting = Process.sortPlan;
            else
                sorting = Process.sortHRN;

            sortAfterAdd(sorting);
        }

        private void Run_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton pressed = (RadioButton)sender;
            if (pressed.Content.Equals("Работает"))
                run = true;
            else
                run = false;
        }

        private void DeleteProcessButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => processesList.Remove((Process)ProcessList.SelectedItem));
            sortAfterAdd(sorting);
        }
    }
}
