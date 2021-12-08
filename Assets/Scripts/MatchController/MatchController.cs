using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchController
    {
    public class MatchController : MonoBehaviour
    {
        private MatchTimeController _matchTimeController;
        private TeamController _teamController;
        private MapData _mapData;
        private Ball _b;
        private GUIStyle _style;

        public GameObject ExplosionParticleSystem;

        void Start()
        {
            _mapData = transform.Find("World").GetComponentInChildren<MapData>();
            _matchTimeController = GetComponent<MatchTimeController>();
            _teamController = GetComponent<TeamController>();
            _teamController.Initialize();
            _b = transform.Find("Ball").GetComponent<Ball>();
            
            _style = new GUIStyle
            {
                normal = {textColor = new Color(1f, 0.25f, 0.15f)},
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
        }

        void Update()
        {
            if (_matchTimeController.MatchTimer < 1.0f && !_matchTimeController.Overtime)
            {
                _matchTimeController.paused = !_b.isTouchedGround;
            }
            if (_matchTimeController.HasEnded() && _mapData.blueScore == _mapData.orangeScore)
            {
                _matchTimeController.ActivateOvertime();
                ResetGameState();
            }
            if(_mapData.isScoredBlue || _mapData.isScoredOrange)
            {
                HandleScoreEvent();
            }
        }

        private void ResetGameState()
        {
            _matchTimeController.paused = true;
            _teamController.SpawnTeams();
            _b.ResetBall();
            _mapData.ResetIsScored();
            _matchTimeController.paused = false;
        }

        private void HandleScoreEvent()
        {
            ResetGameState();
            if (_matchTimeController.Overtime)
            {
                _matchTimeController.EndOvertime();
            }
        }

        private void OnGUI()
        {
            if (_matchTimeController.HasEnded())
            {
                GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height / 2 - 75, 150, 130), "GAME OVER", _style);
            }
        }

        public void HandleDemolition(GameObject demolishedCar)
        {
            TeamController.Team team = _teamController.GetTeamOfCar(demolishedCar);
            StartCoroutine(CarRespawn(demolishedCar, team));
        }

        IEnumerator CarRespawn(GameObject demolishedCar, TeamController.Team team)
        {
            GameObject explosion = Instantiate(ExplosionParticleSystem);
            explosion.transform.position = demolishedCar.transform.position;
            explosion.GetComponent<ParticleSystem>().Play();
            Destroy(explosion, 2f);
            demolishedCar.transform.localPosition = new Vector3(0f, -5f, 0f);
            demolishedCar.GetComponent<InputManager>().enabled = false;
            yield return new WaitForSeconds(3f);
            GetComponent<SpawnController>().SpawnCar(demolishedCar, team, true);
            demolishedCar.GetComponent<InputManager>().enabled = true;
        }

    }

}
