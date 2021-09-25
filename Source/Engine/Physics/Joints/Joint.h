// Copyright (c) 2012-2021 Wojciech Figat. All rights reserved.

#pragma once

#include "Engine/Level/Actor.h"
#include "Engine/Physics/Types.h"
#include "Engine/Scripting/ScriptingObjectReference.h"

class IPhysicsActor;

/// <summary>
/// A base class for all Joint types. Joints constrain how two rigidbodies move relative to one another (for example a door hinge).
/// One of the bodies in the joint must always be movable (non-kinematic and non-static).
/// </summary>
/// <remarks>
/// Joint constraint is created between the parent physic actor (rigidbody, character controller, etc.) and the specified target actor.
/// </remarks>
/// <seealso cref="Actor" />
API_CLASS(Abstract) class FLAXENGINE_API Joint : public Actor
{
DECLARE_SCENE_OBJECT_ABSTRACT(Joint);
protected:

    PxJoint* _joint;
    float _breakForce;
    float _breakTorque;
    Vector3 _targetAnchor;
    Quaternion _targetAnchorRotation;
    bool _enableCollision;

public:

    /// <summary>
    /// The target actor for the joint. It has to be IPhysicsActor type (eg. RigidBody or CharacterController).
    /// </summary>
    API_FIELD(Attributes="EditorOrder(0), DefaultValue(null), EditorDisplay(\"Joint\")")
    ScriptingObjectReference<Actor> Target;

public:

    /// <summary>
    /// Gets the break force. Determines the maximum force the joint can apply before breaking. Broken joints no longer participate in physics simulation.
    /// </summary>
    API_PROPERTY(Attributes="EditorOrder(10), DefaultValue(float.MaxValue), EditorDisplay(\"Joint\")")
    FORCE_INLINE float GetBreakForce() const
    {
        return _breakForce;
    }

    /// <summary>
    /// Sets the break force. Determines the maximum force the joint can apply before breaking. Broken joints no longer participate in physics simulation.
    /// </summary>
    API_PROPERTY() void SetBreakForce(float value);

    /// <summary>
    /// Gets the break torque. Determines the maximum torque the joint can apply before breaking. Broken joints no longer participate in physics simulation.
    /// </summary>
    API_PROPERTY(Attributes="EditorOrder(20), DefaultValue(float.MaxValue), EditorDisplay(\"Joint\")")
    FORCE_INLINE float GetBreakTorque() const
    {
        return _breakTorque;
    }

    /// <summary>
    /// Sets the break torque. Determines the maximum torque the joint can apply before breaking. Broken joints no longer participate in physics simulation.
    /// </summary>
    API_PROPERTY() void SetBreakTorque(float value);

    /// <summary>
    /// Determines whether collision between the two bodies managed by the joint are enabled.
    /// </summary>
    API_PROPERTY(Attributes="EditorOrder(30), DefaultValue(true), EditorDisplay(\"Joint\")")
    FORCE_INLINE bool GetEnableCollision() const
    {
        return _enableCollision;
    }

    /// <summary>
    /// Determines whether collision between the two bodies managed by the joint are enabled.
    /// </summary>
    API_PROPERTY() void SetEnableCollision(bool value);

    /// <summary>
    /// Gets the target anchor.
    /// </summary>
    /// <remarks>
    /// This is the relative pose which locates the joint frame relative to the target actor.
    /// </remarks>
    API_PROPERTY(Attributes="EditorOrder(40), DefaultValue(typeof(Vector3), \"0,0,0\"), EditorDisplay(\"Joint\")")
    FORCE_INLINE Vector3 GetTargetAnchor() const
    {
        return _targetAnchor;
    }

    /// <summary>
    /// Sets the target anchor.
    /// </summary>
    /// <remarks>
    /// This is the relative pose which locates the joint frame relative to the target actor.
    /// </remarks>
    API_PROPERTY() void SetTargetAnchor(const Vector3& value);

    /// <summary>
    /// Gets the target anchor rotation.
    /// </summary>
    /// <remarks>
    /// This is the relative pose rotation which locates the joint frame relative to the target actor.
    /// </remarks>
    API_PROPERTY(Attributes="EditorOrder(50), DefaultValue(typeof(Quaternion), \"0,0,0,1\"), EditorDisplay(\"Joint\")")
    FORCE_INLINE Quaternion GetTargetAnchorRotation() const
    {
        return _targetAnchorRotation;
    }

    /// <summary>
    /// Sets the target anchor rotation.
    /// </summary>
    /// <remarks>
    /// This is the relative pose rotation which locates the joint frame relative to the target actor.
    /// </remarks>
    API_PROPERTY() void SetTargetAnchorRotation(const Quaternion& value);

public:

    /// <summary>
    /// Gets the native PhysX joint object.
    /// </summary>
    FORCE_INLINE PxJoint* GetPhysXJoint() const
    {
        return _joint;
    }

    /// <summary>
    /// Gets the current force applied by the solver to maintain all constraints.
    /// </summary>
    /// <param name="linear">The result linear force.</param>
    /// <param name="angular">The result angular force.</param>
    API_FUNCTION() void GetCurrentForce(API_PARAM(Out) Vector3& linear, API_PARAM(Out) Vector3& angular) const;

public:

    /// <summary>
    /// Creates native join object.
    /// </summary>
    void Create();

public:

    /// <summary>
    /// Occurs when a joint gets broken during simulation.
    /// </summary>
    API_EVENT() Action JointBreak;

    /// <summary>
    /// Called by the physics system when joint gets broken.
    /// </summary>
    virtual void OnJointBreak();

protected:

    struct JointData
    {
        PxPhysics* Physics;
        PxRigidActor* Actor0;
        PxRigidActor* Actor1;
        Quaternion Rot0;
        Quaternion Rot1;
        Vector3 Pos0;
        Vector3 Pos1;
    };

    virtual PxJoint* CreateJoint(JointData& data) = 0;

private:

    void Delete();
    void SetActors();
    void OnTargetChanged();
#if USE_EDITOR
    void DrawPhysicsDebug(RenderView& view);
#endif

public:

    // [Actor]
#if USE_EDITOR
    void OnDebugDrawSelected() override;
#endif
    void Serialize(SerializeStream& stream, const void* otherObj) override;
    void Deserialize(DeserializeStream& stream, ISerializeModifier* modifier) override;

protected:

    // [Actor]
    void BeginPlay(SceneBeginData* data) override;
    void EndPlay() override;
#if USE_EDITOR
    void OnEnable() override;
    void OnDisable() override;
#endif
    void OnActiveInTreeChanged() override;
    void OnParentChanged() override;
    void OnTransformChanged() override;
};
