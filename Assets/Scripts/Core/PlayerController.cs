using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUI spellui;

    public int speed;

    public Unit unit;

    // start is called once before the first Update after the mono is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER, hp);
        StartCoroutine(spellcaster.ManaRegeneration());

        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);

        if (spellui != null && spellcaster.spells.Count > 0)
            spellui.SetSpell(spellcaster.spells[0]);
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) spellcaster.SelectSpell(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) spellcaster.SelectSpell(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) spellcaster.SelectSpell(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) spellcaster.SelectSpell(3);
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>()*speed;
    }

    void Die()
    {
        Debug.Log("You Lost");
    }

    void OnSelectSpell(InputValue value)
    {
        float input = value.Get<float>();
        if (input > 0)
            spellcaster.SelectSpell(spellcaster.selectedSpellIndex + 1);
        else
            spellcaster.SelectSpell(spellcaster.selectedSpellIndex - 1);
    }

}
