using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {


    // 스피드 조정 변수
    [SerializeField]
    private float walkSpeed; //걷기 속도
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;


    private float applySpeed; //속도를 대입하기 위한 함수

    [SerializeField]
    private float jumpForce; // 순간적인 힘으로 얼마나 점프를 할 것인가

    // 상태 변수
    private bool isRun = false; // 걷고 있는지 아닌지
    private bool isCrouch = false;
    private bool isGround = true;

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;
    // 땅 착지 여부
    private CapsuleCollider capsuleCollider;

    //카메라 민감도
    [SerializeField]
    private float lookSensitivity; // 카메라의 민감도

    //카메라 제한
    [SerializeField]
    private float cameraRotationLimit; // 고개의 각도 조절
    private float currentCameraRotationX = 0f; // 정면을 바라보게 기본값 0 

    //필요 컴포넌트
    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid; // 물리적인 요소들이 들어있는 Rugudbody


	void Start () {
        //theCamera = FindObjectOfType<Camera>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>(); //
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;

    }
	void Update () {
        
        TryCrouch();
        IsGround();
        TryJump();
        TryRun();
        Move();
        CameraRotation();
        CharacterRotation();

       
	}
    // 앉기 시도
    private void TryCrouch()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl)){Crouch();}
    }
    // 앉기 동작
    private void Crouch()
    {
        isCrouch = !isCrouch;
        if(isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY =originPosY;
        }
        StartCoroutine(CrouchCoroutine());
        // 아래 코드를 실행시키면 카메라가 부자연스러움 이걸 해결하기 위해 IEnumerator를 사용 
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY , theCamera.transform.localPosition.z);

    }
    //부드러운 앉기 동작 실행
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while(_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0,_posY,0);
            if(count > 15)
            break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0,applyCrouchPosY,0f);
    }
    // 지면 체크
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y+0.1f);
    }
    // 점프 시도
    private void TryJump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGround){Jump();}
    }
    // 점프
    private void Jump()
    {
        // 앉은 상태에서 점프시 앉기 취소
        if(isCrouch)
            Crouch();
        myRigid.velocity = transform.up * jumpForce;
    }
    // 달리기 시도
    private void TryRun()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }
    // 달리기 실행
    private void Running()
    {
        if(isCrouch)
            Crouch();
        isRun = true;
        applySpeed = runSpeed;
    }
    // 달리기 취소 
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    // 움직임 실행
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); // 우측과 좌측 a , d 1 , 0 , -1 좌우 화살표를 받아오기 위함
        float _moveDirZ = Input.GetAxisRaw("Vertical"); // 상 하 움직임

        Vector3 _moveHorizontal = transform.right * _moveDirX; // right 기본 값 : (1 , 0 , 0) x _moveDirX
        Vector3 _moveVertical = transform.forward * _moveDirZ; // forward 기본 값 : (0 , 0 , 1) x _moveDirZ

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        // (1 , 0 , 0) + (0 , 0 , 1) = (1 , 0 , 1) => 2
        // normalized = (0.5 , 0 , 0.5) = 1 방향은 같으나 합이 1이 나오도록 정규화를 시켜주면 1초에 얼마나 이동시킬지 계산이 편해짐
        // * 속도

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime); // 현재위치 + _velocity * 1초동안 움직임 ( 약 0.016 값 )
    }
    // 좌우 캐릭터 회전
    private void CharacterRotation()
    {
        // 좌우 캐릭터 회전
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation , 0f)* lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation*Quaternion.Euler(_characterRotationY));
        Debug.Log(myRigid.rotation);
        Debug.Log(myRigid.rotation.eulerAngles);

    }
    // 상하 카메라 회전
    private void CameraRotation()
    {
        // 상하 카메라 회전
        float _xRotation = Input.GetAxisRaw("Mouse Y"); // 위아래 
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit); // (들어온 값 ,들어온 값 , 최댓값) 최댓값이 넘어버리면 리미트 값으로 고정

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX,0f,0f); //위 아래만 움직일거기에 x값만 고정
    }

}


