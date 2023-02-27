using System;
using System.Threading;
using Microsoft.SPOT;
using System.Text;


using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
//`using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Gadgeteer.Module;

namespace Hero_Arcade_Drive_Example3
{
    public class Program
    {
        /* create PWM controllers 
         note the PWM output are sent to two controller on each side by splitting the PWM data cable.*/
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX victor_right1 = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(1);
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX victor_right2 = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(2);
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX victor_left3 = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(3);
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX victor_left4 = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(4);
        static CTRE.Phoenix.MotorControl.CAN.TalonSRX crusher = new CTRE.Phoenix.MotorControl.CAN.TalonSRX(5);  
        static StringBuilder stringBuilder = new StringBuilder();
        static PneumaticControlModule my_PCM = new PneumaticControlModule(0);
        static CTRE.Phoenix.Controller.GameController m_gamepad = null;
        // IO module stuff
        CTRE.Gadgeteer.Module.DriverModule driver = new DriverModule(CTRE.HERO.IO.Port5);
        //   bool drivelow = DriverModule.OutputState.driveLow;
        //   bool pullup = DriverModule.OutputState.pullUp;
        //        private bool running = false;

        // public bool Running { get => running; set => running = value; }

        public static void Main()
        {
            crusher.ConfigSelectedFeedbackSensor(FeedbackDevice.Analog);
            
            crusher.ConfigForwardSoftLimitThreshold(663, 0);  //833   
            crusher.ConfigReverseSoftLimitThreshold(499, 0);    //533

            //crusher allows for 5 total turns out of 10 total (0,1024) raw units
            
            crusher.ConfigReverseSoftLimitEnable(true, 0);
            crusher.ConfigForwardSoftLimitEnable(true, 0);

            // IO module stuff
            //   CTRE.Gadgeteer.Module.DriverModule driver = new DriverModule(CTRE.HERO.IO.Port5);
            //   bool drivelow = DriverModule.OutputState.driveLow;
            //   bool pullup = DriverModule.OutputState.pullUp;
            //  bool running = false;

            /* loop forever */
            while (true)
            {
                /* drive robot using gamepad */
                Drive();


                Crush();
                //Control digital outputs
                Dout();

                /* print whatever is in our string builder */
                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();

                /* run this task every 20ms */
                Thread.Sleep(20);
            }
        }
        /**
         * If value is within 10% of center, clear it.
         * @param value [out] floating point value to deadband.
         */
        static void Deadband(ref float value)
        {
            if (value < -0.10)
            {
                /* outside of deadband */
            }
            else if (value > +0.10)
            {
                /* outside of deadband */
            }
            else
            {
                /* within 10% so zero it */
                value = 0;
            }
        }
        static void Drive()
        {
            // tests for gamepad. assigns it if found
            if (null == m_gamepad)
                m_gamepad = new GameController(UsbHostDevice.GetInstance());
            /* If the game loses connection the watchdog is not fed and the hero board disables*/
            if (m_gamepad.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
            { /* print the axis value */
                stringBuilder.Append("true ");
                /* allow motor control */
                CTRE.Phoenix.Watchdog.Feed();
                //  running.Equals = true;

            }
            /* The drive controller run at a maxium of 1/2 speed unless the "Turbo" button (Right Tab) is pressed which 
             give full speed to the motor controllers*/
            //7 is left trigger
            bool turbo = m_gamepad.GetButton(6);


            float x = -1 * m_gamepad.GetAxis(1);
            float y = m_gamepad.GetAxis(5);
            if (turbo)
            {//full power
            }
            else
            {
                x = x / 2;
                y = y / 2;
            }

            Deadband(ref x);
            Deadband(ref y);

            float leftThrot = x;
            float rightThrot = y;

            victor_left3.Set(ControlMode.PercentOutput, leftThrot);
            victor_right1.Set(ControlMode.PercentOutput, rightThrot);
            victor_left4.Set(ControlMode.PercentOutput, leftThrot);
            victor_right2.Set(ControlMode.PercentOutput, rightThrot);

            stringBuilder.Append(y + "  ");
            stringBuilder.Append(x);
            stringBuilder.Append("hi");


        }

        static void Crush()
        {
            //min:6 max:1023 (mod 1024)
            //TODO: config to linear potentiometer (not exceed 1024)
            
            
            //print crusher position
            stringBuilder.Append("crusher:");
            stringBuilder.Append(crusher.GetSelectedSensorPosition());


            //crush
            if (m_gamepad.GetButton(7))
            {
                crusher.Set(ControlMode.PercentOutput, .3);
            }

            //release
            else if (m_gamepad.GetButton(8))
            {
                crusher.Set(ControlMode.PercentOutput, -.3);
            }

            else
            {
                crusher.Set(ControlMode.PercentOutput, 0);
            }


        }
        
        static void Dout()
        {

        }

    }
}