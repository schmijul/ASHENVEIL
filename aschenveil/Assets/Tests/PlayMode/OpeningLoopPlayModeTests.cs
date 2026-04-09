using System.Collections;
using Ashenveil.AI;
using Ashenveil.Combat;
using Ashenveil.Inventory;
using Ashenveil.NPC;
using Ashenveil.Player;
using Ashenveil.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Ashenveil.Tests.PlayMode
{
    public class OpeningLoopPlayModeTests
    {
        [UnityTest]
        public IEnumerator GrauwaldScene_LoadsWithIntegratedOpeningLoop()
        {
            yield return SceneManager.LoadSceneAsync("Grauwald", LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindWithTag("Player");
            Assert.That(player, Is.Not.Null);
            Assert.That(player.GetComponent<CombatController>(), Is.Not.Null);
            Assert.That(player.GetComponent<PlayerInventoryRuntime>(), Is.Not.Null);
            Assert.That(player.GetComponent<PlayerInteractionController>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<OpeningLoopBootstrapper>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator CombatAttack_NearbyBoar_ReducesWildlifeHealth()
        {
            yield return SceneManager.LoadSceneAsync("Grauwald", LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindWithTag("Player");
            CombatController combat = player.GetComponent<CombatController>();
            WildlifeHealth boar = Object.FindFirstObjectByType<WildlifeHealth>();

            player.transform.position = boar.transform.position + (Vector3.back * 0.8f);
            player.transform.LookAt(boar.transform.position);
            float startingHealth = boar.CurrentHealth;

            Assert.That(combat.RequestLightAttack(), Is.True);
            yield return new WaitForSeconds(0.35f);

            Assert.That(boar.CurrentHealth, Is.LessThan(startingHealth));
        }

        [UnityTest]
        public IEnumerator WildlifeDeath_SpawnsPickup_AndInteractCollectsIt()
        {
            yield return SceneManager.LoadSceneAsync("Grauwald", LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindWithTag("Player");
            PlayerInventoryRuntime inventory = player.GetComponent<PlayerInventoryRuntime>();
            PlayerInteractionController interaction = player.GetComponent<PlayerInteractionController>();
            WildlifeHealth boar = Object.FindFirstObjectByType<WildlifeHealth>();

            boar.TakeDamage(999f, DamageType.Slash);
            yield return null;
            yield return null;

            InventoryPickup pickup = Object.FindFirstObjectByType<InventoryPickup>();
            Assert.That(pickup, Is.Not.Null);

            player.transform.position = pickup.transform.position + (Vector3.back * 0.25f);
            yield return new WaitForSeconds(0.15f);

            Assert.That(interaction.TryInteract(), Is.True);
            Assert.That(inventory.GetItemCount(pickup.Item), Is.GreaterThanOrEqualTo(1));
        }

        [UnityTest]
        public IEnumerator InteractNearNpc_BeginsConversation()
        {
            yield return SceneManager.LoadSceneAsync("Grauwald", LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindWithTag("Player");
            PlayerInteractionController interaction = player.GetComponent<PlayerInteractionController>();
            NpcController npc = Object.FindFirstObjectByType<NpcController>();

            player.transform.position = npc.transform.position + (Vector3.back * 0.6f);
            yield return new WaitForSeconds(0.15f);

            Assert.That(interaction.TryInteract(), Is.True);
            Assert.That(interaction.IsConversationActive, Is.True);
            Assert.That(interaction.CurrentSpeaker, Is.Not.Empty);
            Assert.That(interaction.CurrentDialogText, Is.Not.Empty);
        }
    }
}
