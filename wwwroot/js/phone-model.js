import * as THREE from 'three';

function roundedRectangle(width, height, radius) {
    const x = -width / 2;
    const y = -height / 2;
    const shape = new THREE.Shape();
    shape.moveTo(x + radius, y);
    shape.lineTo(x + width - radius, y);
    shape.quadraticCurveTo(x + width, y, x + width, y + radius);
    shape.lineTo(x + width, y + height - radius);
    shape.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
    shape.lineTo(x + radius, y + height);
    shape.quadraticCurveTo(x, y + height, x, y + height - radius);
    shape.lineTo(x, y + radius);
    shape.quadraticCurveTo(x, y, x + radius, y);
    return shape;
}

function screenTexture() {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 1024;
    const ctx = canvas.getContext('2d');
    const background = ctx.createLinearGradient(0, 0, 500, 1024);
    background.addColorStop(0, '#270d21');
    background.addColorStop(.42, '#800020');
    background.addColorStop(.74, '#4b1b61');
    background.addColorStop(1, '#0b0b0e');
    ctx.fillStyle = background;
    ctx.fillRect(0, 0, 512, 1024);

    for (const [x, y, radius, color] of [
        [355, 260, 300, 'rgba(220,76,111,.5)'],
        [120, 690, 350, 'rgba(123,44,191,.48)'],
        [380, 850, 260, 'rgba(58,134,255,.32)']
    ]) {
        const glow = ctx.createRadialGradient(x, y, 0, x, y, radius);
        glow.addColorStop(0, color);
        glow.addColorStop(1, 'rgba(0,0,0,0)');
        ctx.fillStyle = glow;
        ctx.fillRect(0, 0, 512, 1024);
    }
    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    return texture;
}

export function createPhone(color = '#800020') {
    const group = new THREE.Group();
    const bodyMaterial = new THREE.MeshPhysicalMaterial({
        color,
        metalness: .9,
        roughness: .2,
        clearcoat: 1,
        clearcoatRoughness: .09
    });
    const darkMetal = new THREE.MeshPhysicalMaterial({ color: '#140c18', metalness: .65, roughness: .22, clearcoat: 1 });
    const lensGlass = new THREE.MeshPhysicalMaterial({ color: '#161126', metalness: .3, roughness: .13, clearcoat: 1 });
    const glowMaterial = new THREE.MeshBasicMaterial({ color: '#e0647f' });
    const texture = screenTexture();

    const bodyGeometry = new THREE.ExtrudeGeometry(roundedRectangle(2, 3.85, .24), {
        depth: .19,
        steps: 1,
        curveSegments: 10,
        bevelEnabled: true,
        bevelThickness: .045,
        bevelSize: .035,
        bevelSegments: 3
    });
    bodyGeometry.center();
    group.add(new THREE.Mesh(bodyGeometry, bodyMaterial));

    const screen = new THREE.Mesh(
        new THREE.PlaneGeometry(1.83, 3.66),
        new THREE.MeshPhysicalMaterial({ map: texture, metalness: .08, roughness: .12, clearcoat: 1, clearcoatRoughness: .04 })
    );
    screen.position.z = .17;
    group.add(screen);

    const island = new THREE.Mesh(new THREE.BoxGeometry(.5, .075, .018), darkMetal);
    island.position.set(0, 1.69, .187);
    group.add(island);

    const backPanel = new THREE.Mesh(new THREE.PlaneGeometry(1.83, 3.66), bodyMaterial);
    backPanel.position.z = -.17;
    backPanel.rotation.y = Math.PI;
    group.add(backPanel);

    const cameraPlate = new THREE.Mesh(new THREE.BoxGeometry(.81, 1.11, .08), darkMetal);
    cameraPlate.position.set(-.45, 1.17, -.23);
    group.add(cameraPlate);
    for (const [x, y] of [[-.66, 1.48], [-.26, 1.48], [-.66, 1.06]]) {
        const rim = new THREE.Mesh(new THREE.CylinderGeometry(.145, .145, .045, 32), bodyMaterial);
        rim.rotation.x = Math.PI / 2;
        rim.position.set(x, y, -.29);
        group.add(rim);
        const lens = new THREE.Mesh(new THREE.CylinderGeometry(.105, .105, .05, 32), lensGlass);
        lens.rotation.x = Math.PI / 2;
        lens.position.set(x, y, -.325);
        group.add(lens);
    }
    const flash = new THREE.Mesh(new THREE.CircleGeometry(.055, 20), glowMaterial);
    flash.position.set(-.26, 1.06, -.305);
    flash.rotation.y = Math.PI;
    group.add(flash);

    for (const y of [.68, .28]) {
        const button = new THREE.Mesh(new THREE.BoxGeometry(.035, .28, .065), bodyMaterial);
        button.position.set(-1.04, y, 0);
        group.add(button);
    }
    const power = new THREE.Mesh(new THREE.BoxGeometry(.035, .45, .065), bodyMaterial);
    power.position.set(1.04, .42, 0);
    group.add(power);

    return {
        group,
        setColor(value) {
            bodyMaterial.color.set(/^#[0-9a-f]{6}$/i.test(value) ? value : '#800020');
        },
        dispose() {
            group.traverse(object => {
                object.geometry?.dispose();
                if (object.material && object.material !== bodyMaterial) object.material.dispose();
            });
            bodyMaterial.dispose();
            texture.dispose();
        }
    };
}
