using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Week007_CS_ParticleSystem : MonoBehaviour
{
	const int numThreads = 8;
	public int MaxParticleCount = 100;

	public float emitPerSecond = 1;
	float m_emitPerSecondRemain;

	public Vector3 initVelocity = new Vector3(0, 10, 0);
	public Vector3 initVelocityVariant = new Vector3(1, 0.2f, 1);

	public float initLifespan = 5;
	public float initLifespanVariant = 2;

	public float gravity = -9.8f;
	public float TimeScale = 1;

	public GameObject ColliderPlane;

	public ComputeShader computeShader;
	public Material material;
	public Mesh mesh;

	ComputeBuffer _particlePosition;
	ComputeBuffer _particleVelocity;
	ComputeBuffer _particleLifespan;
	ComputeBuffer _particleNoise;
	const int _particleNoiseCount = 512;

	[Header("-- Debug --")]
	public int m_activeParticleCount;
	public int m_particleIndex;

	static void ReleaseBuffer(ref ComputeBuffer buf) {
		if (buf != null) {
			buf.Release();
			buf = null;
		}
	}

	void ReleaseAllBuffers() {
		ReleaseBuffer(ref _particlePosition);
		ReleaseBuffer(ref _particleVelocity);
		ReleaseBuffer(ref _particleLifespan);
		ReleaseBuffer(ref _particleNoise);
	}

	static int RoundUpToMultiple(int v, int n) {
		return (v + n - 1) / n * n;
	}

	public class MyRenderPass : ScriptableRenderPass {
		Week007_CS_ParticleSystem _owner;
		public MyRenderPass(Week007_CS_ParticleSystem owner) {
			renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
			_owner = owner;
		}
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			_owner.OnExecuteDraw(context, ref renderingData);
		}
	}

	MyRenderPass myRenderPass;

	ComputeBuffer createComputeBuffer<T>(int count) {
		int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
		return new ComputeBuffer(count * stride, stride);
	}

	private void OnEnable()  {
		myRenderPass = new MyRenderPass(this);
		MyPostProcessManager.instance.OnAddRenderPasses += AddRenderPasses;
	}
	private void OnDisable() {
		MyPostProcessManager.instance.OnAddRenderPasses -= AddRenderPasses;
		ReleaseAllBuffers();
	}

	void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
		renderer.EnqueuePass(myRenderPass);
	}

	void Start() {
		Application.targetFrameRate = 60;

		if (!computeShader) return;

		int RoundUpParticleCount = RoundUpToMultiple(MaxParticleCount, numThreads);
		if (RoundUpParticleCount <= 0) return;

		_particlePosition = createComputeBuffer<Vector3>(RoundUpParticleCount);
		_particleVelocity = createComputeBuffer<Vector3>(RoundUpParticleCount);
		_particleLifespan = createComputeBuffer<Vector3>(RoundUpParticleCount);

		_particleNoise    = createComputeBuffer<Vector3>(_particleNoiseCount);
		var noise = new Vector3[_particleNoiseCount];
		for (int i = 0; i < noise.Length; i++) {
			noise[i] = Random.insideUnitSphere;
		}
		_particleNoise.SetData(noise);
	}

	void Update()
	{
		if (!computeShader) return;

		m_emitPerSecondRemain += Time.deltaTime * emitPerSecond;

		int newParticleCount = (int)m_emitPerSecondRemain;
		m_emitPerSecondRemain -= newParticleCount;

		int newParticleStart = m_particleIndex;
		int newParticleEnd   = (m_particleIndex + newParticleCount) % MaxParticleCount;

		m_particleIndex += newParticleCount;
		m_particleIndex %= MaxParticleCount;
		m_activeParticleCount = Mathf.Max(m_particleIndex, m_activeParticleCount);

		if (m_activeParticleCount <= 0) return;

		int kernelIndex = computeShader.FindKernel("CSMain");

		computeShader.SetVector("initVelocity", initVelocity);
		computeShader.SetVector("initVelocityVariant", initVelocityVariant);
		computeShader.SetFloat("initLifespan", initLifespan);
		computeShader.SetFloat("initLifespanVariant", initLifespanVariant);

		computeShader.SetFloat("deltaTime", Time.deltaTime * TimeScale);
		computeShader.SetFloat("gravity", gravity);

		computeShader.SetBuffer(kernelIndex, "_particlePosition", _particlePosition);
		computeShader.SetBuffer(kernelIndex, "_particleVelocity", _particleVelocity);
		computeShader.SetBuffer(kernelIndex, "_particleLifespan", _particleLifespan);
		computeShader.SetBuffer(kernelIndex, "_particleNoise",    _particleNoise);
		computeShader.SetInt("_particleNoiseCount", _particleNoiseCount);

		computeShader.SetInt("m_activeParticleCount", m_activeParticleCount);
		computeShader.SetInt("newParticleStart", newParticleStart);
		computeShader.SetInt("newParticleEnd",   newParticleEnd);

		if (ColliderPlane) {
			var planeNormal = ColliderPlane.transform.up;
			Vector4 plane = planeNormal;
			plane.w = Vector3.Dot(ColliderPlane.transform.position, planeNormal);
			computeShader.SetVector("ColliderPlane", plane);
		}

		computeShader.Dispatch(kernelIndex, RoundUpToMultiple(m_activeParticleCount, numThreads), 1, 1);
	}

	void OnExecuteDraw(ScriptableRenderContext context, ref RenderingData renderingData) {
		if (!material || !mesh) return;
		if (m_activeParticleCount <= 0) return;

		material.SetBuffer("_particlePosition", _particlePosition);
		material.SetBuffer("_particleLifespan", _particleLifespan);

		CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
		cmd.DrawMeshInstancedProcedural(mesh, 0, material, 0, m_activeParticleCount);
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
	}
}
