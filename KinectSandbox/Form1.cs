﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Kinect;
using Midi;

namespace KinectSandbox
{
    public partial class Form1 : Form
    {
        KinectSensor kinectSensor = null;
        BodyFrameReader bodyFrameReader = null;
        Body[] bodies = null;

        HandState lastLeftState = HandState.Open;
        HandState lastRightState = HandState.Open;

        OutputDevice outputDevice;

        public void InitialiseMidi(int deviceIndex)
        {
            outputDevice = OutputDevice.InstalledDevices[deviceIndex];
            if (outputDevice.IsOpen)
            {
                outputDevice.Close();
                outputDevice.Open();
            }
            else
            {
                outputDevice.Open();
            }
        }

        public void CloseOutput()
        {
            if (outputDevice != null && outputDevice.IsOpen) outputDevice.Close();
        }

        public void OnDisable()
        {
            if (outputDevice != null && outputDevice.IsOpen) outputDevice.Close();
        }

        public void InitialiseKinect()
        {
            kinectSensor = KinectSensor.GetDefault();

            if(kinectSensor != null)
            {
                kinectSensor.Open();
            }

            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();

            if(bodyFrameReader != null)
            {
                bodyFrameReader.FrameArrived += Reader_FrameArrived;
            }
        }

        public void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if(bodyFrame != null)
                {
                    if(bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }
            }
            if (dataReceived)
            {
                foreach (Body body in bodies)
                {
                    if(body.IsTracked)
                    {
                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        Joint leftHand = joints[JointType.HandLeft];
                        Joint rightHand = joints[JointType.HandRight];

                        if ((body.HandLeftState != HandState.Unknown) && (body.HandLeftState != HandState.NotTracked))
                        {
                            lastLeftState = body.HandLeftState;
                        }
                        if ((body.HandRightState != HandState.Unknown) && (body.HandRightState != HandState.NotTracked))
                        {
                            lastRightState = body.HandRightState;
                        }

                        label1.Text = "leftX: " + leftHand.Position.X.ToString("##.##");
                        label2.Text = "leftY: " + leftHand.Position.Y.ToString("##.##");
                        label3.Text = "leftZ: " + leftHand.Position.Z.ToString("##.##");
                        label4.Text = "LeftState: " + lastLeftState;
                        label5.Text = "RightState: " + lastRightState;
                        MakeMusic(lastLeftState, lastRightState, leftHand.Position.X, leftHand.Position.Y, rightHand.Position.X, rightHand.Position.Y);
                    }
                }
            }
        }

        public void MakeMusic(HandState left, HandState right, float leftY, float leftX, float rightY, float rightX)
        {
            outputDevice.SendNoteOn(Channel.Channel1, Pitch.C4, 80);
        }

        public Form1()
        {
            InitializeComponent();
            InitialiseKinect();
            InitialiseMidi(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            outputDevice.SendNoteOn(Channel.Channel1, Pitch.C4, 80);
            outputDevice.SendNoteOff(Channel.Channel1, Pitch.C4, 80);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CloseOutput();
            InitialiseMidi(comboBox1.SelectedIndex);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach(OutputDevice device in OutputDevice.InstalledDevices)
            {
                comboBox1.Items.Add(device.Name);
            }
        }
    }
}
