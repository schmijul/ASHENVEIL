using Ashenveil.Core;
using Ashenveil.UI;
using UnityEngine;

namespace Ashenveil.Flow
{
    /// <summary>
    /// Central runtime glue for the demo. Owns the <see cref="DemoFlowModel"/>, advances
    /// it from gameplay signals (kills, aether touch, boss death), drives tutorial toasts
    /// per phase, swaps the village to its burning state, and plays the escape finale.
    /// Wired entirely through serialized references by the scene builder.
    /// Referenced GDD section: Demo-Ablauf.
    /// </summary>
    public sealed class DemoDirector : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private HudController _hud;
        [SerializeField] private FadeScreenController _fade;
        [SerializeField] private EndScreenController _endScreen;

        [Header("World state roots")]
        [SerializeField] private GameObject _villageNormalRoot;
        [SerializeField] private GameObject _villageBurningRoot;

        [Header("Tuning")]
        [SerializeField] private int _killsToFinishHunt = 1;

        private readonly DemoFlowModel _flow = new DemoFlowModel();
        private int _kills;

        /// <summary>
        /// The demo flow state machine.
        /// </summary>
        public DemoFlowModel Flow => _flow;

        private void Awake()
        {
            if (_villageBurningRoot != null)
            {
                _villageBurningRoot.SetActive(false);
            }

            _flow.PhaseChanged += OnPhaseChanged;
            _flow.DemoEnded += OnDemoEnded;

            if (_endScreen != null)
            {
                _endScreen.Bind(direction => _flow.ChooseEscape(direction));
            }
        }

        private void OnEnable()
        {
            GameSignals.AnimalKilled += OnAnimalKilled;
            GameSignals.AetherTouched += OnAetherTouched;
            GameSignals.BossDefeated += OnBossDefeated;
        }

        private void OnDisable()
        {
            GameSignals.AnimalKilled -= OnAnimalKilled;
            GameSignals.AetherTouched -= OnAetherTouched;
            GameSignals.BossDefeated -= OnBossDefeated;
        }

        private void Start()
        {
            // Kick off the opening beat.
            EnqueueToast("wake", "Du erwachst im Wald. Folge dem Pfad zum Dorf. (WASD, Maus)");
        }

        /// <summary>
        /// Called by the village entry trigger when the player reaches the village.
        /// </summary>
        public void NotifyEnteredVillage()
        {
            _flow.JumpTo(DemoPhase.EnterVillage);
        }

        /// <summary>
        /// Called by the deep-forest trigger when the player nears the crystal clearing.
        /// </summary>
        public void NotifyEnteredDeepForest()
        {
            _flow.JumpTo(DemoPhase.DeepForestCrystal);
        }

        /// <summary>
        /// Called when the player accepts/starts the village side quests.
        /// </summary>
        public void NotifyVillageQuestsStarted()
        {
            _flow.AdvanceFrom(DemoPhase.EnterVillage);
        }

        private void OnAnimalKilled(string animalId, Vector3 position)
        {
            _kills++;
            if (_flow.Phase == DemoPhase.WakeInForest)
            {
                _flow.AdvanceFrom(DemoPhase.WakeInForest); // first blood begins the hunt
            }

            if (_flow.Phase == DemoPhase.Hunt && _kills >= _killsToFinishHunt)
            {
                EnqueueToast("hunt_done", "Gut gejagt. Bring Fleisch und Felle ins Dorf.");
            }
        }

        private void OnAetherTouched(Vector3 position, float amount)
        {
            EnqueueToast("aether", "Deine Hände leuchten... der Äther verletzt dich nicht. Du bist anders.");
            _flow.AdvanceFrom(DemoPhase.DeepForestCrystal); // proceed toward the boss
        }

        private void OnBossDefeated(string bossId)
        {
            _flow.AdvanceFrom(DemoPhase.BossFight);
        }

        private void OnPhaseChanged(DemoPhase previous, DemoPhase next)
        {
            GameSignals.RaisePhaseChanged((int)previous, (int)next);

            switch (next)
            {
                case DemoPhase.Hunt:
                    EnqueueToast("combat", "Greife mit Linksklick an, blocke mit Rechtsklick, weiche mit Alt aus.");
                    break;
                case DemoPhase.EnterVillage:
                    EnqueueToast("trade", "Sprich mit Händlern (E), öffne das Inventar mit Tab.");
                    break;
                case DemoPhase.DeepForestCrystal:
                    EnqueueToast("crystal", "Ein Ätherkristall pulsiert im tiefen Wald. Berühre ihn (E).");
                    break;
                case DemoPhase.BossFight:
                    EnqueueToast("boss", "Der mutierte Wolf! Nur äther-verstärkte Schläge (Q) verletzen ihn.");
                    break;
                case DemoPhase.BurningVillage:
                    BeginBurningVillage();
                    break;
                case DemoPhase.EscapeChoice:
                    BeginEscape();
                    break;
            }
        }

        private void BeginBurningVillage()
        {
            EnqueueToast("burning", "Das Dorf brennt! Kehre zurück.");
            if (_fade != null)
            {
                _fade.FadeToBlack(() =>
                {
                    SwapToBurningVillage();
                    _fade.FadeFromBlack();
                    // After showing the burning village, offer the escape.
                    _flow.AdvanceFrom(DemoPhase.BurningVillage);
                }, 1.2f);
            }
            else
            {
                SwapToBurningVillage();
                _flow.AdvanceFrom(DemoPhase.BurningVillage);
            }
        }

        private void SwapToBurningVillage()
        {
            if (_villageNormalRoot != null)
            {
                _villageNormalRoot.SetActive(false);
            }

            if (_villageBurningRoot != null)
            {
                _villageBurningRoot.SetActive(true);
            }
        }

        private void BeginEscape()
        {
            if (_endScreen != null)
            {
                _endScreen.Show();
            }
        }

        private void OnDemoEnded(EscapeDirection direction)
        {
            // The end screen shows its own thanks message; nothing else to do.
            Debug.Log($"[DemoDirector] Demo ended, direction: {direction}");
        }

        private void EnqueueToast(string id, string germanText)
        {
            _hud?.Toasts.Enqueue(id, germanText);
        }
    }
}
