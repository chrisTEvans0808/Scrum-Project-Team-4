using System;
using System.Diagnostics;
using TowerDefense_TheRPG.code;
using TowerDefense_TheRPG.Properties;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace TowerDefense_TheRPG
{
    public partial class FrmMain : Form
    {
        #region Fields
        private Player player;
        private Village village;
        private List<Enemy> enemies;
        private List<Arrow> arrows;
        private List<PowerUp> powerups = new List<PowerUp>();
        private Boolean ActivePowerUp = false;
        private string storyLine;
        private int curStoryLineIndex;
        private Random rand;
        private int PlayerDirX = 0, PlayerDirY = 0;
        private bool FiringArrows = false;
        public static int kills;
        public static int counter;
        public static string storeTimePlayed = "";
        private int LevelBefore;
        private int LevelAfter;
        private string arrow_directions = "horizontal";
        #endregion

        #region Methods
        #region Ctor
        public FrmMain()
        {
            InitializeComponent();
            FormManager.PushToFormStack(this);
            DoubleBuffered = true;
            ControlManager.ResMan = Resources.ResourceManager;
            ControlManager.Form = this;
            rand = new Random();
        }
        #endregion

        #region Event functions
        // timers
        private void tmrTextCrawl_Tick(object sender, EventArgs e)
        {
            if (curStoryLineIndex < storyLine.Length)
            {
                lblStoryLine.Text += storyLine[curStoryLineIndex++];
                lblStoryLine.Refresh();
            }
            else
            {
                tmrTextCrawl.Enabled = false;
            }
        }
        private void tmrGameTime_Tick(object sender, EventArgs e)
        {

            counter++;
            //format timer
            TimeSpan time = TimeSpan.FromSeconds(counter);
            lblCountTime.Text = time.ToString(@"mm\:ss");
            storeTimePlayed = time.ToString(@"mm\:ss");
            //generate boss balloon every 30 seconds
            GenEnemyPos(out int x, out int y);
            Enemy bossBalloon;
            if (counter % 30 == 0)
            {
                bossBalloon = Enemy.MakeBossBalloon(x, y);
                enemies.Add(bossBalloon);
            }
            if (player.CurHealth <= 0){
                counter= 0;
                kills = 0;
                tmrGameTime.Enabled = false;
                tmrGameTime.Enabled = false;
            }
        }
        private void tmrSpawnEnemies_Tick(object sender, EventArgs e)
        {
            GenEnemyPos(out int x, out int y);
            int enemyType = rand.Next(4);
            Enemy balloon;
            switch (enemyType)
            {
                case 0:
                    balloon = Enemy.MakeRedBalloon(x, y);
                    break;
                case 1:
                    balloon = Enemy.MakePurpleBalloon(x, y);
                    break;
                case 2:
                    balloon = Enemy.MakeGrayBalloon(x, y);
                    break;
                default:
                    balloon = Enemy.MakeOrangeBalloon(x, y);
                    break;

            }
            enemies.Add(balloon);
        }

        private void tmrMoveEnemies_Tick(object sender, EventArgs e)
        {
            MoveEnemies();
        }
        private void tmrSpawnArrows_Tick(object sender, EventArgs e)
        {
            FireArrows();
        }
        private void tmrSpawnPowerUp_Tick(object sender, EventArgs e)
        {
            SpawnPowerUps();
        }
        private void tmrCheckPowerUpsCollected_Tick(object sender, EventArgs e)
        {
            CheckPowerUps();
        }
        private void tmrPowerUpsDeactivate_Tick(object sender, EventArgs e)
        {
            if (player.Attack == 1.0f && player.MaxHealth == 10.0f)
            {
                player.Attack = 0.15f;
                player.MaxHealth = 3.0f;
                player.CurHealth = (player.CurHealth / 10.0f) * player.MaxHealth;
            }
            else if (player.MoveSpeed == 20)
            {
                player.MoveSpeed = 10;
            }
            tmrPowerUpsDeactivate.Enabled = false;
            lblSpeedActivated.Visible = false;
            lblStrengthActivated.Visible = false;
            ActivePowerUp = false;
            tmrSpawnPowerUp.Enabled = true;
        }
        private void tmrMoveArrows_Tick(object sender, EventArgs e)
        {
            MoveArrows();
        }
        private void FiredArrow_Tick(object sender, EventArgs e)
        {
            tmrFiredArrow.Enabled = false;
        }

        // Originally created by Christian Evans. Altered by Eban Goodman-Blue.
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            {
                if (e.KeyChar == 'e' && tmrFiredArrow.Enabled == false)
                {
                    FireArrows();
                    tmrFiredArrow.Enabled = true;
                }
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    PlayerDirY = -1;
                    break;
                case Keys.Down:
                case Keys.S:
                    PlayerDirY = +1;
                    break;
                case Keys.Left:
                case Keys.A:
                    PlayerDirX = -1;
                    break;
                case Keys.Right:
                case Keys.D:
                    PlayerDirX = +1;
                    break;
                case Keys.E:
                    FiringArrows = true;
                    break;
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    PlayerDirY = 0;
                    break;
                case Keys.Down:
                case Keys.S:
                    PlayerDirY = 0;
                    break;
                case Keys.Left:
                case Keys.A:
                    PlayerDirX = 0;
                    break;
                case Keys.Right:
                case Keys.D:
                    PlayerDirX = 0;
                    break;
                case Keys.E:
                    FiringArrows = false;
                    break;
            }
        }
        // buttons
        private void btnStart_Click(object sender, EventArgs e)
        {
            GameStart(sender, e);
        }
        private void btnTutorial_Click(object sender, EventArgs e)
        {
            if (btnTutorial.Text == "Tutorial")
            {
                BackgroundImage = Resources.tutorial;
                btnStoryLine.Visible = false;
                btnStart.Visible = false;
                btnTutorial.Text = "Exit Tutorial";

                tmrSpawnEnemies.Enabled = false;
                tmrMoveEnemies.Enabled = false;
                tmrMoveArrows.Enabled = false;
            }
            else
            {
                BackgroundImage = Resources.title;
                btnStoryLine.Visible = true;
                btnStart.Visible = true;
                btnTutorial.Text = "Tutorial";
            }
        }

        /// <summary>
        /// Initializes game logic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameStart(object sender, EventArgs e)
        {
            BackgroundImage = null;
            btnStart.Visible = false;
            btnTutorial.Visible = false;
            btnStart.Enabled = false;
            btnTutorial.Enabled = false;
            btnStoryLine.Visible = false;
            lblStoryLine.Visible = false;

            //start game timer
            tmrGameTime.Start();

            //make kills, money, and timer visible (both labels and values)
            //make kills, money, level, and timer visible
            lblCountKills.Visible = true;
            lblCountTime.Visible = true;
            lblCountMoney.Visible = true;
            lblLevelValue.Visible = true;
            lblKills.Visible = true;
            lblGameTime.Visible = true;
            lblMoney.Visible = true;
            lblLevel.Visible = true;

            //makes Shop buttons visible (both labels and buttons)
            shopLabel.Visible = true;
            townHeal.Visible = true;
            villageHealCost.Visible = true;
            playerHeal.Visible = true;
            playerHealCost.Visible = true;
            arrowInc.Visible = true;
            arrowIncCost.Visible = true;
            

            enemies = new List<Enemy>();
            arrows = new List<Arrow>();
            player = new Player(Width / 2, Height / 2 + 100);
            village = new Village(Width / 2 - 80, Height / 2 - 50);
            village.ControlContainer.SendToBack();
            tmrSpawnEnemies.Enabled = true;
            tmrMoveEnemies.Enabled = true;
            tmrMoveArrows.Enabled = true;
            tmrSpawnPowerUp.Enabled = true;
            tmrCheckPowerUpsCollected.Enabled = true;
            if (player.Level == 1) {
                tmrSpawnArrows.Interval = 2000;
                tmrSpawnArrows.Enabled = false;
            }
            tmrTextCrawl.Enabled = false;

            // TODO: setting the background image here causes visual defects as enemies and player move
            //       around the screen. Consider either fixing these defects or setting BackgroundImage to null
            BackgroundImage = Resources.ground;

            // important, keep this call to Focus()!
            // otherwise, for whatever reason, the start button retains focus (even when enabled = false)
            // and arrow key presses are ignored and won't move player.
            Focus();
        }
        /// <summary>
        /// Runs when an Enemy dies, handles all XP, kill counter, and money logic.
        /// </summary>
        /// <param name="enemy"></param>
        private void EnemyKilled(Enemy enemy)
        {
            enemy.Hide();
            
            // increase kill counter
            kills += 1;
            lblCountKills.Text = kills.ToString();

            // increase money counter by amount balloon drops
            player.GainMoney(enemy.MoneyGiven);
            lblCountMoney.Text = player.Money.ToString();

            // increase XP by amount balloon drops
            LevelBefore = player.Level;
            player.GainXP(enemy.XPGiven);
            LevelAfter = player.Level;

            if (LevelAfter > LevelBefore)
            {
                lblLevelValue.Text = LevelAfter.ToString();
                
                UpgradeVillage();

                if (tmrSpawnEnemies.Interval >= 10000)
                {
                    tmrSpawnEnemies.Interval -= 3000;
                }
                else if (tmrSpawnEnemies.Interval >= 3000)
                {
                    tmrSpawnEnemies.Interval -= 1000;
                }
            }

            if (LevelAfter >= 5 && BackgroundImage != null)
            {
                BackgroundImage = null;
                tmrEndlessTextHide.Enabled = true;
                EndlessMode.Visible = true;
            }

        }

        private void btnStoryLine_Click(object sender, EventArgs e)
        {
            if (btnStoryLine.Text.StartsWith("Show"))
            {
                Storyline();
                BackgroundImage = null;
                btnStart.Visible = false;
                btnTutorial.Visible = false;
                btnStoryLine.Text = "Hide Storyline";
                lblStoryLine.Visible = true;

                tmrSpawnEnemies.Enabled = false;
                tmrMoveEnemies.Enabled = false;
                tmrMoveArrows.Enabled = false;
                tmrTextCrawl.Enabled = true;
            }
            else
            {
                BackgroundImage = Resources.title;
                btnStart.Visible = true;
                btnTutorial.Visible = true;
                btnStoryLine.Text = "Show Storyline";
                lblStoryLine.Visible = false;

                tmrTextCrawl.Enabled = false;
            }
        }
        #endregion

        #region Helper functions
        private void Storyline()
        {
            // Altered by Christian Evans.
            storyLine = "Once upon a time, there was this village called Taon. ";
            storyLine += "The village was home to medieval golems that took the form of miniature buildings. ";
            storyLine += "Life was fruitful and bliss... until the balloons showed up! ";
            storyLine += "The golems were unequipped for such a featherweight foe, unable to protect themselves ";
            storyLine += "from their aerial onslaught; however, a hero would rise among the ranks, capable ";
            storyLine += "of defeating these floating enemies. This is the hero's story, as he takes on ";
            storyLine += "waves of these balloons. Go forth, Peaches the Tower!";
            lblStoryLine.Text = "";
            tmrTextCrawl.Enabled = true;
            curStoryLineIndex = 0;
        }
        public void GenEnemyPos(out int x, out int y)
        {
            int enterDir = rand.Next(4);
            const int offscreen = -5;
            switch (enterDir)
            {
                case 0: // left
                    y = rand.Next(0, Height);
                    x = -offscreen;
                    break;
                case 1: // bottom
                    x = rand.Next(0, Width);
                    y = Height + offscreen;
                    break;
                case 2: // right
                    y = rand.Next(0, Height);
                    x = Width + offscreen;
                    break;
                default: // top
                    x = rand.Next(0, Width);
                    y = -offscreen;
                    break;
            }
        }
        private void MoveEnemies()
        {
            foreach (var enemy in enemies)
            {
                if (enemy.CurHealth <= 0)
                {
                    continue;
                }
                int xDir = 0;
                int yDir = 0;
                if (enemy.ControlContainer.Left < Width / 2)
                {
                    xDir = 1;
                }
                else
                {
                    xDir = -1;
                }
                if (enemy.ControlContainer.Top < Height / 2)
                {
                    yDir = 1;
                }
                else
                {
                    yDir = -1;
                }
                enemy.Move(xDir, yDir, true, Width, Height);
                if (enemy.DidCollide(player))
                {
                    enemy.TakeDamageFrom(player);
                    player.TakeDamageFrom(enemy);
                    player.UpdateHealth();
                    if(player.CurHealth <= 0)
                    {
                        village.Hide(); // defeated
                        Form frmStat = new FrmStats();
                        counter = 0;
                        frmStat.Show();                        
                        this.Hide();
                        FormManager.PushToFormStack(frmStat);

                        // disable timers
                        tmrMoveArrows.Enabled = false;
                        tmrMoveEnemies.Enabled = false;
                        tmrSpawnArrows.Enabled = false;

                        tmrSpawnEnemies.Enabled = false;
                        tmrSpawnPowerUp.Enabled = false;
                    }

                    if (enemy.CurHealth <= 0)
                    {
                        EnemyKilled(enemy);
                    }
                    else
                    {
                        enemy.KnockBack();
                    }
                }
                else if (enemy.DidCollide(village))
                {
                    village.TakeDamageFrom(enemy);
                    if (village.CurHealth <= 0)
                    {
                        village.Hide(); // defeated
                        Form frmGO = new FrmGameOver();
                        frmGO.Show();
                        this.Hide();
                        FormManager.PushToFormStack(frmGO);

                        // disable timers
                        tmrMoveArrows.Enabled = false;
                        tmrMoveEnemies.Enabled = false;
                        tmrSpawnArrows.Enabled = false;

                        tmrSpawnEnemies.Enabled = false;
                        tmrSpawnPowerUp.Enabled = false;
                    }
                    else
                    {
                        enemy.KnockBack();
                    }
                }
            }

            List<Enemy> enemiesToRemove = new List<Enemy>();
            foreach (Enemy enemy in enemies)
            {
                if (enemy.CurHealth <= 0)
                {
                    enemiesToRemove.Add(enemy);
                }
            }

            foreach (Enemy enemy in enemiesToRemove)
            {
                enemies.Remove(enemy);
            }
        }
        private void MoveArrows()
        {
            List<Arrow> arrowsToRemove = new List<Arrow>();
            foreach (Arrow arrow in arrows)
            {
                arrow.Move();
                foreach (Enemy enemy in enemies)
                {
                    if (arrow.ArrowCollide(enemy))
                    {
                        enemy.TakeDamage(0.1f);
                        if (enemy.CurHealth <= 0)
                        {
                            EnemyKilled(enemy);
                        }
                        else
                        {
                            enemy.KnockBack();
                        }
                        arrowsToRemove.Add(arrow);
                    }
                }
            }
            foreach (Arrow arrow in arrowsToRemove)
            {
                arrows.Remove(arrow);
                Controls.Remove(arrow.ControlCharacter);
            }
        }

        // Altered by Christian Evans.
        private void FireArrows()
        {
            Arrow arrowLeft = new Arrow(player.X, player.Y, -1, 0);
            Arrow arrowRight = new Arrow(player.X, player.Y, +1, 0);
            // New arrow direction types.
            Arrow arrowUp = new Arrow(player.X, player.Y, 0, +1);
            Arrow arrowDown = new Arrow(player.X, player.Y, 0, -1);
            Arrow arrowTopRight = new Arrow(player.X, player.Y, +1, +1);
            Arrow arrowBotRight = new Arrow(player.X, player.Y, +1, -1);
            Arrow arrowTopLeft = new Arrow(player.X, player.Y, -1, +1);
            Arrow arrowBotLeft = new Arrow(player.X, player.Y, -1, -1);

            arrows.Add(arrowLeft);
            arrows.Add(arrowRight);
      
            arrowLeft.ControlCharacter.BringToFront();
            arrowRight.ControlCharacter.BringToFront();
           
            // For when the player purchases the first arrow upgrade.
            if (arrow_directions == "cardinal") {
                arrows.Add(arrowUp);
                arrows.Add(arrowDown);

                arrowLeft.ControlCharacter.BringToFront();
                arrowRight.ControlCharacter.BringToFront();
                arrowUp.ControlCharacter.BringToFront();
                arrowDown.ControlCharacter.BringToFront();
            }

            // For when the player purchases the second arrow upgrade.
            if (arrow_directions == "intercardinal") {
                arrows.Add(arrowUp);
                arrows.Add(arrowDown);
                arrows.Add(arrowTopRight);
                arrows.Add(arrowBotRight);
                arrows.Add(arrowTopLeft);
                arrows.Add(arrowBotLeft);

                arrowLeft.ControlCharacter.BringToFront();
                arrowRight.ControlCharacter.BringToFront();
                arrowUp.ControlCharacter.BringToFront();
                arrowDown.ControlCharacter.BringToFront();
                arrowTopRight.ControlCharacter.BringToFront();
                arrowBotRight.ControlCharacter.BringToFront();
                arrowTopLeft.ControlCharacter.BringToFront();
                arrowBotLeft.ControlCharacter.BringToFront();
            }
        }
        public void SpawnPowerUps()
        {
            if(powerups.Count() == 0 && ActivePowerUp == false) {
                int y = rand.Next(10, 800);
                int x = rand.Next(10, 1256);
                int PowerUpType = rand.Next(3);
                PowerUp powerup;
                switch (PowerUpType)
                {
                    case 0:
                        powerup = new PowerUp("healing", x, y);
                        break;
                    case 1:
                        powerup = new PowerUp("speed", x, y);
                        break;
                    default:
                        powerup = new PowerUp("strength", x, y);
                        break;
                }
                powerups.Add(powerup);
                powerup.ControlPowerUp.BringToFront();
                tmrPowerUpDecay.Enabled = true;
            }
        }
        public void CheckPowerUps()
        {
            List<PowerUp> PowerUpsToRemove = new List<PowerUp>();
            foreach (PowerUp powerup in powerups)
            {
                if (powerup.DidCollide(player))
                {
                    {
                        if (powerup.Name == "healing")
                        {
                            player.IncreaseHealth(player.MaxHealth - player.CurHealth);
                            player.UpdateHealth();
                        }
                        else if (powerup.Name == "strength")
                        {
                            player.Attack = 1.0f;
                            player.MaxHealth = 10.0f;
                            player.IncreaseHealth(player.MaxHealth - player.CurHealth);
                            player.UpdateHealth();
                            lblStrengthActivated.Visible = true;
                            tmrPowerUpsDeactivate.Enabled = true;
                            ActivePowerUp = true;
                            tmrSpawnPowerUp.Enabled = false;
                        }
                        else
                        {
                            player.MoveSpeed = 20;
                            lblSpeedActivated.Visible = true;
                            tmrPowerUpsDeactivate.Enabled = true;
                            ActivePowerUp = true;
                            tmrSpawnPowerUp.Enabled = false;
                        }
                        PowerUpsToRemove.Add(powerup);
                    }
                }
            }

            foreach (PowerUp power in PowerUpsToRemove)
            {
                powerups.Remove(power);
                Controls.Remove(power.ControlPowerUp);
            }
        }
        private void PlayerMove(int DirX, int DirY)
        {
            if(DirX !=0 || DirY != 0)
            {
                player.Move(DirX, DirY, true, Width, Height);
            }
        }
        /// <summary>
        /// Upgrades the Village's Health, called by EnemyKilled if a player levels up.
        /// </summary>
        private void UpgradeVillage()
        {
                village.UpdateMaxHealth(1.0f);
        }

        #endregion

        #endregion

        private void tmrFiringArrows_Tick(object sender, EventArgs e)
        {
            if (FiringArrows || tmrFiredArrow.Enabled == false)
            {
                FireArrows();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void lblStoryLine_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_2(object sender, EventArgs e) {

        }

        /// <summary>
        /// Button uses heal_town_image.png and is for healing the town when the town is not at max health.
        /// Made by Christian Evans.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e) {
            if (village.CurHealth != village.MaxHealth) {
                if (player.Money >= 3) {
                    Activate();
                    village.HealVillage();
                    village.UpdateHealth();
                    player.GainMoney(-3);
                    lblCountMoney.Text = player.Money.ToString();
                }
            }
            this.ActiveControl = null;
        }

        /// <summary>
        /// Button uses heal_player_image and is for healing the player when the player is not at max health.
        /// Is a caution as originally the only way the player could heal was from power-ups.
        /// Made by Christian Evans.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_1(object sender, EventArgs e) {
            if (player.CurHealth != player.MaxHealth) {
                if (player.Money >= 2) {
                    Activate();
                    player.IncreaseHealth(player.MaxHealth - player.CurHealth);
                    player.UpdateHealth();
                    player.GainMoney(-2);
                    lblCountMoney.Text = player.Money.ToString();
                }   
            }
            this.ActiveControl = null;
        }

        private void FrmMain_Load(object sender, EventArgs e) {

        }

        private void label3_Click(object sender, EventArgs e) {

        }

        private void label2_Click(object sender, EventArgs e) {

        }

        private void arrowIncCost_Click(object sender, EventArgs e) {

        }

        /// <summary>
        /// Button uses vertical_arrows.png and is for upgrading the player arrows to include vertical arrows.
        /// Disappears after button is clicked, revealing the second arrow upgrade and its cost.
        /// Made by Christian Evans.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void arrowInc_Click(object sender, EventArgs e) {
            if (player.Money >= 8) {
                Activate();
                arrow_directions = "cardinal";
                player.GainMoney(-8);
                lblCountMoney.Text = player.Money.ToString();
                arrowIncOmniCost.Visible = true;
                arrowIncOmni.Visible = true;
                arrowIncCost.Visible = false;
                arrowInc.Visible = false;
            }
            this.ActiveControl = null;
        }

        /// <summary>
        /// Button uses omnidir_arrows.png and is for upgrading the player arrows to include intercardinal arrows.
        /// Disappears after button is clicked.
        /// Made by Christian Evans.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void arrowIncOmni_Click(object sender, EventArgs e) {
            if (player.Money >= 15)
            {
                Activate();
                arrow_directions = "intercardinal";
                player.GainMoney(-15);
                lblCountMoney.Text = player.Money.ToString();
                arrowIncOmniCost.Visible = false;
                arrowIncOmni.Visible = false;
                this.ActiveControl = null;
            }
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void EndlessTextHideTick(object sender, EventArgs e)
        {
            EndlessMode.Visible = false;
            tmrEndlessTextHide.Enabled = false;
        }

        private void label1_Click_3(object sender, EventArgs e)
        {

        }

        private void tmrPowerUpDecay_Tick(object sender, EventArgs e)
        {
            if(powerups.Count() == 1)
            {
                foreach (PowerUp power in powerups.ToList())
                {
                    powerups.Remove(power);
                    Controls.Remove(power.ControlPowerUp);
                }
            }
            tmrPowerUpDecay.Enabled = false;
        }

        private void tmrMovePlayer_Tick(object sender, EventArgs e)
        {
            PlayerMove(PlayerDirX, PlayerDirY);
        }
    }
}