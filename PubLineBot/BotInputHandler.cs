using HoldfastGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PubLineBot
{
    public class BotPlayerInputHandler : IInputManager
    {
        // Token: 0x060000CA RID: 202 RVA: 0x00043C5C File Offset: 0x00041E5C
        public BotPlayerInputHandler(bool botAutoMove)
        {
            this.autoMoveBot = botAutoMove;
            int num = 0;
            dfList<int> dfList = dfList<int>.Obtain();
            int count = this.UsableActionsSet.Count;
            this.autonomousPlayerKeys = new AutonomousPlayerKey[count];
            this.keyActionsLastEnabled = new Dictionary<int, AutonomousPlayerKey>(count, Int32EqualityComparer.Default);
            foreach (PlayerInputAction playerInputAction in this.UsableActionsSet)
            {
                int num2 = (int)playerInputAction;
                AutonomousPlayerKey autonomousPlayerKey = new AutonomousPlayerKey
                {
                    LastToggledTime = Time.time,
                    lastState = true
                };
                this.keyActionsLastEnabled[num2] = autonomousPlayerKey;
                dfList.Add(num2);
                this.autonomousPlayerKeys[num++] = autonomousPlayerKey;
            }
            this.keyActionsIDs = dfList.ToArray();
            dfList.Release();
        }

        // Token: 0x060000CB RID: 203 RVA: 0x00043DBC File Offset: 0x00041FBC
        public void ManualUpdate()
        {
            for (int i = 0; i < this.autonomousPlayerKeys.Length; i++)
            {
                AutonomousPlayerKey lastKey = this.autonomousPlayerKeys[i];
                this.UpdateInput(lastKey);
            }
            this.UpdateAxis();
        }

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x060000CC RID: 204 RVA: 0x00008049 File Offset: 0x00006249
        public string Name
        {
            get
            {
                return "Autonomous!";
            }
        }

        // Token: 0x17000011 RID: 17
        // (get) Token: 0x060000CD RID: 205 RVA: 0x00008050 File Offset: 0x00006250
        // (set) Token: 0x060000CE RID: 206 RVA: 0x00008058 File Offset: 0x00006258
        public string SpriteName { get; private set; }

        // Token: 0x17000012 RID: 18
        // (get) Token: 0x060000CF RID: 207 RVA: 0x00008061 File Offset: 0x00006261
        public Vector2 Axis
        {
            get
            {
                return this.currentAxis;
            }
        }

        // Token: 0x17000013 RID: 19
        // (get) Token: 0x060000D0 RID: 208 RVA: 0x00008069 File Offset: 0x00006269
        public Vector2 ShipAxis
        {
            get
            {
                return this.Axis;
            }
        }

        // Token: 0x17000014 RID: 20
        // (get) Token: 0x060000D1 RID: 209 RVA: 0x00008069 File Offset: 0x00006269
        public Vector2 CannonControlAxis
        {
            get
            {
                return this.Axis;
            }
        }

        // Token: 0x060000D2 RID: 210 RVA: 0x00043DF8 File Offset: 0x00041FF8
        private void UpdateAxis()
        {
            if (!this.autoMoveBot)
            {
                this.currentAxis = Vector2.zero;
                return;
            }
            int num = (!this.GetPlayerActionKey(PlayerInputAction.Left)) ? ((!this.GetPlayerActionKey(PlayerInputAction.Right)) ? 0 : 1) : -1;
            int num2 = (!this.GetPlayerActionKey(PlayerInputAction.Down)) ? ((!this.GetPlayerActionKey(PlayerInputAction.Up)) ? 0 : 1) : -1;
            this.currentAxis = new Vector2((float)num, (float)num2);
        }

        // Token: 0x17000015 RID: 21
        // (get) Token: 0x060000D3 RID: 211 RVA: 0x00008071 File Offset: 0x00006271
        public Vector2 MousePosition
        {
            get
            {
                return this.UpdateMousePosition();
            }
        }

        // Token: 0x17000016 RID: 22
        // (get) Token: 0x060000D4 RID: 212 RVA: 0x00043E78 File Offset: 0x00042078
        public float AxisSpeed
        {
            get
            {
                Vector2 axis = this.Axis;
                return (float)((axis.x == 0f && axis.y == 0f) ? 0 : 1);
            }
        }

        // Token: 0x17000017 RID: 23
        // (get) Token: 0x060000D5 RID: 213 RVA: 0x00043EB8 File Offset: 0x000420B8
        public float MouseScrollWheelValue
        {
            get
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                {
                    return 0f;
                }
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                {
                    return 1f;
                }
                return -1f;
            }
        }

        // Token: 0x060000D6 RID: 214 RVA: 0x00008079 File Offset: 0x00006279
        public bool GetKeyDown(KeyCode keyCode)
        {
            return false;
        }

        // Token: 0x060000D7 RID: 215 RVA: 0x0000807C File Offset: 0x0000627C
        public bool GetKeyDown(PlayerInputAction playerAction)
        {
            return this.GetPlayerActionKey(playerAction);
        }

        // Token: 0x060000D8 RID: 216 RVA: 0x00008079 File Offset: 0x00006279
        public bool GetKey(KeyCode keyCode)
        {
            return false;
        }

        // Token: 0x060000D9 RID: 217 RVA: 0x0000807C File Offset: 0x0000627C
        public bool GetKey(PlayerInputAction playerAction)
        {
            return this.GetPlayerActionKey(playerAction);
        }

        // Token: 0x060000DA RID: 218 RVA: 0x00008079 File Offset: 0x00006279
        public bool AnyKeyPressed()
        {
            return false;
        }

        // Token: 0x060000DB RID: 219 RVA: 0x00008085 File Offset: 0x00006285
        public string GetSprite(PlayerInputAction playerAction)
        {
            throw new NotImplementedException();
        }

        // Token: 0x060000DC RID: 220 RVA: 0x00008079 File Offset: 0x00006279
        public bool GetMouseButton(int button)
        {
            return false;
        }

        // Token: 0x060000DD RID: 221 RVA: 0x00008079 File Offset: 0x00006279
        public bool GetMouseButtonDown(int button)
        {
            return false;
        }

        // Token: 0x17000018 RID: 24
        // (get) Token: 0x060000DE RID: 222 RVA: 0x0000808C File Offset: 0x0000628C
        // (set) Token: 0x060000DF RID: 223 RVA: 0x00008094 File Offset: 0x00006294
        public InputLayouts InputLayout { get; private set; }

        // Token: 0x060000E0 RID: 224 RVA: 0x0000809D File Offset: 0x0000629D
        public bool GetKeyboardNumberPressed(out int number)
        {
            number = 0;
            if (UnityEngine.Random.Range(0f, 1f) > 0.3f)
            {
                return false;
            }
            number = UnityEngine.Random.Range(1, 10);
            return true;
        }

        // Token: 0x060000E1 RID: 225 RVA: 0x00043F08 File Offset: 0x00042108
        private bool GetPlayerActionKey(PlayerInputAction action)
        {
            AutonomousPlayerKey autonomousPlayerKey;
            return this.keyActionsLastEnabled.TryGetValue((int)action, out autonomousPlayerKey) && autonomousPlayerKey.lastState;
        }

        // Token: 0x060000E2 RID: 226 RVA: 0x00043F30 File Offset: 0x00042130
        private Vector2 UpdateMousePosition()
        {
            float num = Time.time - this.lastUpdatedMousePosition;
            if (num < this.currentMousePositionWait)
            {
                this.currentMousePosition = Vector2.Lerp(this.currentMousePosition, Vector2.zero, Time.deltaTime);
                return this.currentMousePosition;
            }
            this.currentMousePosition = new Vector2(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-5f, 5f));
            this.lastUpdatedMousePosition = Time.time;
            this.currentMousePositionWait = UnityEngine.Random.Range(1f, 2f);
            return this.currentMousePosition;
        }

        // Token: 0x060000E3 RID: 227 RVA: 0x00043FC8 File Offset: 0x000421C8
        private void UpdateInput(AutonomousPlayerKey lastKey)
        {
            float num = Time.time - lastKey.LastToggledTime;
            if (num < lastKey.TimeBeforeNextToggle)
            {
                return;
            }
            float num2 = 0.3f;
            bool lastState = UnityEngine.Random.Range(0f, 1f) < num2;
            lastKey.LastToggledTime = Time.time;
            lastKey.lastState = lastState;
            float timeBeforeNextToggle = UnityEngine.Random.Range(0.1f, 0.9f);
            lastKey.TimeBeforeNextToggle = timeBeforeNextToggle;
        }

        // Token: 0x040000B5 RID: 181
        private readonly Dictionary<int, AutonomousPlayerKey> keyActionsLastEnabled;

        // Token: 0x040000B6 RID: 182
        private readonly int[] keyActionsIDs;

        // Token: 0x040000B7 RID: 183
        private readonly AutonomousPlayerKey[] autonomousPlayerKeys;

        // Token: 0x040000B8 RID: 184
        private float lastUpdatedMousePosition;

        // Token: 0x040000B9 RID: 185
        private Vector2 currentMousePosition;

        // Token: 0x040000BA RID: 186
        private float currentMousePositionWait;

        // Token: 0x040000BB RID: 187
        private readonly bool autoMoveBot;

        // Token: 0x040000BD RID: 189
        private Vector2 currentAxis;

        // Token: 0x040000BE RID: 190
        private readonly HashSet<PlayerInputAction> UsableActionsSet = new HashSet<PlayerInputAction>(PlayerInputActionEqualityComparer.Default)
        {
            PlayerInputAction.Up,
            PlayerInputAction.Down,
            PlayerInputAction.Left,
            PlayerInputAction.Right,
            PlayerInputAction.Run,
            PlayerInputAction.FireFirearm,
            PlayerInputAction.ToggleCombatStance,
            PlayerInputAction.Interact,
            PlayerInputAction.Jump,
            PlayerInputAction.ToggleAimFirearm,
            PlayerInputAction.ReloadFirearm
        };
    }
}
