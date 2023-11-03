using System;
using System.Collections.Generic;
using Model;
using Service;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class DisplayCharacters : InjectableBehaviour
    {
        [Inject] private ConfigService _config;
        [Inject] private AgentsModel _model;

        struct PlaceholderInfo
        {
            public Vector3 pos;
            public Quaternion rot;
        }

        private readonly List<PlaceholderInfo> _placeholders = new();

        private void Start()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var placeholder = transform.GetChild(i);

                _placeholders.Add(new PlaceholderInfo()
                {
                    pos = placeholder.localPosition,
                    rot = placeholder.localRotation
                });
            }

            _model.Updated.Add(RenderChars);
            RenderChars();
        }

        private void RenderChars()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            var agentConfigs = _config.GetAgents();

            Debug.Log("-------");

            for (byte i = 0; i < _model.Get().Length; i++)
            {
                var agent = _model.Get()[i];
                var prefab = agentConfigs[agent.Id].prefab;

                if (string.IsNullOrEmpty(prefab)) continue;

                var characterObj = Instantiate(Resources.Load<GameObject>("Characters/" + prefab), _placeholders[i].pos,
                    _placeholders[i].rot, transform);
                characterObj.gameObject.name = prefab;


                Debug.Log(prefab + ":" + i + " loc " + _placeholders[i].pos);

                var dialogue = characterObj.GetComponentInChildren<DialogueTrigger>();
                dialogue.Cooldown = agent.Cooldown;
                dialogue.Id = i;
                dialogue.UpdateCurrentQuest();
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(RenderChars);
        }
    }
}