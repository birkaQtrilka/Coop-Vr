using Coop_Vr.Networking;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Coop_Vr
{
    public class ClientBuff
    {
        TcpClient _client;

        List<Score> _scores = new List<Score>();
        Score _lastAddedPlayerScore = null;
        DemoShow demo = new();

        public ClientBuff()
        {

            connectToServer();
        }

        void connectToServer()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect("192.168.1.157", 5555);
                Console.WriteLine("Connected to server.");
            }
            catch (Exception e)
            {
                //_highScoreView.SetPlayerScoreHeader("Not connected to server.");
                Console.WriteLine(e.Message);
            }
        }

        void onScoreAdded(Score pScore)
        {
            //store the last score the player wants to add locally
            _lastAddedPlayerScore = pScore;

            //send the add score command
            AddRequestExample addRequest = new();
            addRequest.score = pScore;
            sendObject(addRequest);
        }


        void sendObject(ISerializable pOutObject)
        {
            try
            {
                Console.WriteLine("Sending:" + pOutObject);

                Packet outPacket = new Packet();
                outPacket.Write(pOutObject);

                StreamUtil.Write(_client.GetStream(), outPacket.GetBytes());
            }

            catch (Exception e)
            {
                //for quicker testing, we reconnect if something goes wrong.
                Console.WriteLine(e.Message);
                _client.Close();
                connectToServer();
            }
        }

        public void Step()
        {
            try
            {
                if (_client.Available > 0)
                {
                    byte[] inBytes = StreamUtil.Read(_client.GetStream());
                    Packet inPacket = new(inBytes);
                    ISerializable inObject = inPacket.ReadObject();

                    if (inObject is GetScoresExample getScores) { handleHighScores(getScores); }
                }

                demo.Step();
            }
            catch (Exception e)
            {
                //for quicker testing, we reconnect if something goes wrong.
                Console.WriteLine(e.Message);
                _client.Close();
                connectToServer();
            }
        }

        void handleHighScores(GetScoresExample pHighscoresUpdate)
        {
            //new
            _scores = pHighscoresUpdate.scores;

            //same as previous
            _scores.Sort((b, a) => a.score.CompareTo(b.score));

            //do we have the highscore? (this assumes unique playernames etc)
            bool highScore =
                (_scores.Count > 0) &&
                (_lastAddedPlayerScore != null) &&
                (_scores[0].name == _lastAddedPlayerScore.name && _scores[0].score == _lastAddedPlayerScore.score);

            //_highScoreView.SetPlayerScoreHeader(highScore ? "!!! NEW HIGHSCORE !!!" : "YOUR SCORE");
            //_highScoreView.SetPlayerScore(_lastAddedPlayerScore);
            //_highScoreView.SetHighScores(_scores);
        }
    }
}
