using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace My_book_10
{
    /// <summary>
    /// Логика взаимодействия для SaveIndicator.xaml
    /// </summary>
    public partial class SaveIndicator : UserControl
    {
        public enum SaveState
        {
            ReadyToSave,
            Saving,
            Saved
        }

        public event Action OnSaveRequested;

        private SaveState _currentState = SaveState.ReadyToSave;

        public SaveIndicator()
        {
            InitializeComponent();
            SetState(SaveState.ReadyToSave);
        }

        public void SetState(SaveState state)
        {
            switch (state)
            {
                case SaveState.ReadyToSave:
                    StopSpinning();
                    StateText.Visibility = Visibility.Visible;
                    StateIcon.Source = null;
                    StateText.Text = TryFindResource("si.readytosave") as string;
                    break;
                case SaveState.Saving:
                    StartSpinning();
                    StateIcon.Source = TryFindResource("si.Saving") as ImageSource;
                    StateText.Visibility = Visibility.Collapsed;
                    break;
                case SaveState.Saved:
                    StopSpinning();
                    StateText.Visibility = Visibility.Visible;
                    StateIcon.Source = TryFindResource("si.Saved") as ImageSource;
                    StateText.Text = TryFindResource("si.saved") as string;
                    break;
            }
        }

        private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentState == SaveState.ReadyToSave)
            {
                SetState(SaveState.Saving); // Переход в состояние "сохраняется"
                OnSaveRequested?.Invoke();
            }
        }

        // метод для установки состояния после сохранения:
        public void NotifySaved()
        {
            SetState(SaveState.Saved);
        }

        private void AnimateToCurrentWidth()
        {
            // Получаем ширину, которую займет контент
            RootBorder.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double newWidth = RootBorder.DesiredSize.Width;
            double currentWidth = RootBorder.ActualWidth;

            var animation = new DoubleAnimation
            {
                From = currentWidth,
                To = newWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            RootBorder.BeginAnimation(WidthProperty, animation);
        }
        private Storyboard _spinStoryboard;

        private void StartSpinning()
        {
            if (_spinStoryboard != null)
                return;

            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(2)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            _spinStoryboard = new Storyboard();
            _spinStoryboard.Children.Add(rotateAnimation);

            Storyboard.SetTarget(rotateAnimation, SpinnerRotate);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath(RotateTransform.AngleProperty));

            _spinStoryboard.Begin();
        }

        private void StopSpinning()
        {
            if (_spinStoryboard != null)
            {
                _spinStoryboard.Stop();
                _spinStoryboard = null;
                SpinnerRotate.Angle = 0; // сброс угла
            }
        }
    }
}
