using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraOgre : MonoBehaviour
{
	public static CameraOgre Instance = null;
	public static bool mouseEdgeOfScreenMoveEnabled = true;
	public static bool mouseButtonMoveEnabled = true;
	public static bool keysMoveEnabled = true;

	Vector3 prevMousePos;
	public float mouseMoveSpeed;
	public float keysMoveSpeed;
	public float mouseEdgeOfScreenMoveSpeed;
	public float verticalMultiplier = 1f;

	[Tooltip("How far from edge of screen can mouse be when movement activates. In pixels.")]
	public int mouseEdgeOfScreenBias;

	public GameObject cameraPlane;

	public Vector2 cameraMinLimit;
	public Vector2 cameraMaxLimit;

	public bool smoothJumpToPosition = true;
	public float smoothJumpSpeed = 10f;
	private Vector3 smoothJumpTargetPosition;
	private bool smoothJumping = false;

	private Camera gameCamera;
	private Direction cameraRotation = Direction.Up;

	public delegate void CameraEvent();
	public event CameraEvent CameraTurned;

	public Dictionary<Direction, Vector2Int> directionVectors = new Dictionary<Direction, Vector2Int>()
	{ { Direction.Right, new Vector2Int(1, 0) }, { Direction.Left, new Vector2Int(-1, 0) }, { Direction.Up, new Vector2Int(0, 1) }, { Direction.Down, new Vector2Int(0, -1) }
	};

	public enum Direction
	{
		Up,
		Right,
		Down,
		Left
	}

	public Direction GetRandomDirection()
	{
		return (Direction) Random.Range(0, 4);
	}

	public Vector2Int GetRandomDirectionVector()
	{
		return directionVectors[GetRandomDirection()];
	}

	void Start()
	{
		prevMousePos = Input.mousePosition;
	}

	void LateUpdate()
	{
		MoveCamera();
	}

	public int GetHiddenLevelObjectsLayer()
	{
		return LayerMask.NameToLayer("HiddenLevelObjects");
	}

	public int GetVisibleLevelObjectsLayer()
	{
		return LayerMask.NameToLayer("VisibleLevelObjects");
	}

	public int GetVisibleCharactersLayer()
	{
		return LayerMask.NameToLayer("VisibleCharacters");
	}

	public Direction GetAbsoluteDirection(Vector2 vector)
	{
		Vector2 vectorNormalized = vector.normalized;
		Vector2Int vectorInt = new Vector2Int(Mathf.RoundToInt(vectorNormalized.x), Mathf.RoundToInt(vectorNormalized.y));

		if (directionVectors[Direction.Up] == vectorInt)
		{
			return Direction.Up;
		}

		if (directionVectors[Direction.Left] == vectorInt)
		{
			return Direction.Left;
		}

		if (directionVectors[Direction.Down] == vectorInt)
		{
			return Direction.Down;
		}

		if (directionVectors[Direction.Right] == vectorInt)
		{
			return Direction.Right;
		}

		Debug.LogWarning("Could not figure out direction of vector.");
		return Direction.Up;
	}

	public Vector2Int GetUpDirection()
	{
		switch (cameraRotation)
		{
			case Direction.Up:
				return directionVectors[Direction.Up];
			case Direction.Right:
				return directionVectors[Direction.Right];
			case Direction.Down:
				return directionVectors[Direction.Down];
			case Direction.Left:
				return directionVectors[Direction.Left];
			default:
				return directionVectors[Direction.Up];
		}
	}

	public Vector2Int GetRightDirection()
	{
		switch (cameraRotation)
		{
			case Direction.Up:
				return directionVectors[Direction.Right];
			case Direction.Right:
				return directionVectors[Direction.Down];
			case Direction.Down:
				return directionVectors[Direction.Left];
			case Direction.Left:
				return directionVectors[Direction.Up];
			default:
				return directionVectors[Direction.Right];
		}
	}

	public Vector2Int GetDownDirection()
	{
		switch (cameraRotation)
		{
			case Direction.Up:
				return directionVectors[Direction.Down];
			case Direction.Right:
				return directionVectors[Direction.Left];
			case Direction.Down:
				return directionVectors[Direction.Up];
			case Direction.Left:
				return directionVectors[Direction.Right];
			default:
				return directionVectors[Direction.Down];
		}
	}

	public Vector2Int GetLeftDirection()
	{
		switch (cameraRotation)
		{
			case Direction.Up:
				return directionVectors[Direction.Left];
			case Direction.Right:
				return directionVectors[Direction.Up];
			case Direction.Down:
				return directionVectors[Direction.Right];
			case Direction.Left:
				return directionVectors[Direction.Down];
			default:
				return directionVectors[Direction.Left];
		}
	}

	public void JumpToPosition(Vector3 position)
	{
		// find suitable position for camera to look at given position
		cameraPlane.SetActive(true);
		RaycastHit hit;
		LayerMask mask = LayerMask.GetMask("CameraPlane");

		Vector3 targetPosition = transform.position;

		if (Physics.Raycast(position, -transform.forward, out hit, 100f, mask))
		{
			targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
		}
		cameraPlane.SetActive(false);

		if (smoothJumpToPosition)
		{
			smoothJumpTargetPosition = targetPosition;
			smoothJumping = true;
		}
		else
		{
			transform.position = targetPosition;
		}
	}

	private void MoveCamera()
	{
		if (GameUI.Instance.IsFullScreenWindowOpen())
		{
			return;
		}

		// Moving smoothly to a target position
		if (smoothJumping)
		{
			Vector3 movement = (smoothJumpTargetPosition - transform.position).normalized * smoothJumpSpeed * Time.deltaTime;
			// If closer than one frame to target position, snap to target position and end movement
			if (Vector3.Distance(transform.position + movement, smoothJumpTargetPosition) < smoothJumpSpeed * Time.deltaTime)
			{
				transform.position = smoothJumpTargetPosition;
				smoothJumping = false;
			}
			else
			{
				transform.Translate(movement, Space.World);
			}
		}
		else
		{
			if (mouseButtonMoveEnabled)
			{
				MoveCameraWithMouseDrag();
			}

			if (mouseEdgeOfScreenMoveEnabled)
			{
				MoveCameraWithMouseMovement();
			}

			if (keysMoveEnabled)
			{
				MoveCameraWithKeys();
			}

			SnapToLimits();
		}
	}

	private void SnapToLimits()
	{
		if (transform.position.x < cameraMinLimit.x)
		{
			Vector3 pos = transform.position;
			pos.x = cameraMinLimit.x;
			transform.position = pos;
		}
		else if (transform.position.x > cameraMaxLimit.x)
		{
			Vector3 pos = transform.position;
			pos.x = cameraMaxLimit.x;
			transform.position = pos;
		}

		if (transform.position.z < cameraMinLimit.y)
		{
			Vector3 pos = transform.position;
			pos.z = cameraMinLimit.y;
			transform.position = pos;
		}
		else if (transform.position.z > cameraMaxLimit.y)
		{
			Vector3 pos = transform.position;
			pos.z = cameraMaxLimit.y;
			transform.position = pos;
		}
	}

	/// <summary>
	/// Moves camera when mouse is touching side of screen
	/// </summary>
	private void MoveCameraWithMouseMovement()
	{
		float x = 0f;
		float y = 0f;
		if (Input.mousePosition.x >= Screen.width - mouseEdgeOfScreenBias)
		{
			x = 1f;
		}
		else if (Input.mousePosition.x < mouseEdgeOfScreenBias)
		{
			x = -1f;
		}

		if (Input.mousePosition.y >= Screen.height - mouseEdgeOfScreenBias)
		{
			y = 1f;
		}
		else if (Input.mousePosition.y < mouseEdgeOfScreenBias)
		{
			y = -1f;
		}

		Vector3 mouseMove = new Vector3(x, 0f, y);
		mouseMove = Quaternion.Euler(0, -45f, 0f) * mouseMove;
		mouseMove.z *= verticalMultiplier;

		transform.Translate(mouseEdgeOfScreenMoveSpeed * mouseMove, Space.World);
	}

	private void MoveCameraWithKeys()
	{
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		Vector3 keysCameraMove = new Vector3(horizontal, 0f, vertical);
		keysCameraMove = Quaternion.Euler(0, -45f, 0f) * keysCameraMove;
		keysCameraMove.z *= verticalMultiplier;

		transform.Translate(keysMoveSpeed * keysCameraMove, Space.World);
	}

	private void MoveCameraWithMouseDrag()
	{
		// Moving camera by holding down middle mouse button
		if (Input.GetButtonDown("MouseMoveKey"))
		{
			prevMousePos = Input.mousePosition;
		}

		if (Input.GetButton("MouseMoveKey"))
		{
			Vector3 delta = (Input.mousePosition - prevMousePos);
			Vector3 mouseCameraMove = new Vector3(delta.x, 0f, delta.y);
			mouseCameraMove = Quaternion.Euler(0, -45f, 0f) * mouseCameraMove;
			mouseCameraMove.z *= verticalMultiplier;

			transform.Translate(mouseMoveSpeed * mouseCameraMove, Space.World);
		}
		prevMousePos = Input.mousePosition;
	}

	protected virtual void OnCameraTurned()
	{
		if (CameraTurned != null)
		{
			CameraTurned.Invoke();
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
			Destroy(this);
			return;
		}

		gameCamera = GetComponent<Camera>();
	}
}
