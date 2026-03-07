using System;
using UnityEngine;

namespace Code.Server
{
    public class NetCommandSender : MonoBehaviour
    {
        [SerializeField] private NetClient netClient;
        [SerializeField] private CommandQueue commandQueue;

        private void Update()
        {
            if (netClient == null || commandQueue == null) return;
            if (!netClient.IsConnected) return;

            // відправляємо всі команди, що накопичились за кадр
            while (commandQueue.TryDequeue(out var cmd))
            {
                if (cmd == null) break;

                // наразі підтримуємо MoveCommand
                if (cmd is MoveCommand move)
                {
                    var msg = new CmdMoveMsg
                    {
                        type = "cmd_move",
                        unitId = move.EntityId,
                        x = move.Target.x,
                        y = move.Target.y
                    };

                    string json = JsonUtility.ToJson(msg);
                    netClient.SendLine(json);
                }
                else
                {
                    Debug.LogWarning($"NetCommandSender: unsupported command type {cmd.GetType().Name}");
                }
            }
        }

        [Serializable]
        private class CmdMoveMsg
        {
            public string type;
            public int unitId;
            public float x;
            public float y;
        }
    }
}