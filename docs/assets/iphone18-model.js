import * as THREE from 'three';

// A site-made visualization of the Burgundy Pro Max design, not an Apple 3D asset.
function roundedRect(width, height, radius) {
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

function wallpaper() {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 1024;
    const ctx = canvas.getContext('2d');
    const base = ctx.createLinearGradient(50, 0, 430, 1024);
    base.addColorStop(0, '#190b17');
    base.addColorStop(.43, '#4c2334');
    base.addColorStop(1, '#1b0c19');
    ctx.fillStyle = base;
    ctx.fillRect(0, 0, 512, 1024);

    const ribbons = [
        { y: 220, bend: 260, color: '#c37c91' },
        { y: 490, bend: -190, color: '#ad5b77' },
        { y: 755, bend: 230, color: '#e09aaf' }
    ];
    ctx.lineCap = 'round';
    for (const ribbon of ribbons) {
        const glow = ctx.createLinearGradient(0, ribbon.y - 180, 512, ribbon.y + 180);
        glow.addColorStop(0, 'rgba(24, 9, 21, 0)');
        glow.addColorStop(.38, ribbon.color);
        glow.addColorStop(.53, '#6c354b');
        glow.addColorStop(1, 'rgba(24, 9, 21, 0)');
        ctx.beginPath();
        ctx.moveTo(-130, ribbon.y - 110);
        ctx.bezierCurveTo(90, ribbon.y + ribbon.bend, 415, ribbon.y - ribbon.bend, 645, ribbon.y + 100);
        ctx.strokeStyle = glow;
        ctx.lineWidth = 180;
        ctx.shadowColor = '#e69bb2';
        ctx.shadowBlur = 44;
        ctx.stroke();
        ctx.shadowBlur = 0;
        ctx.strokeStyle = 'rgba(255, 190, 207, .55)';
        ctx.lineWidth = 3;
        ctx.stroke();
    }
    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    return texture;
}

export function createIPhone18ProMax() {
    const group = new THREE.Group();
    const body = new THREE.MeshPhysicalMaterial({ color: '#79384b', metalness: .76, roughness: .28, clearcoat: .55 });
    const backGlass = new THREE.MeshPhysicalMaterial({ color: '#8a475c', metalness: .24, roughness: .34, clearcoat: .8 });
    const cameraPlate = new THREE.MeshPhysicalMaterial({ color: '#925166', metalness: .55, roughness: .3, clearcoat: .85 });
    const rim = new THREE.MeshPhysicalMaterial({ color: '#bd7f92', metalness: .79, roughness: .21 });
    const dark = new THREE.MeshPhysicalMaterial({ color: '#080a10', metalness: .27, roughness: .22, clearcoat: 1 });
    const lens = new THREE.MeshPhysicalMaterial({ color: '#111723', metalness: .35, roughness: .12, clearcoat: 1 });
    const glass = new THREE.MeshBasicMaterial({ color: '#0b0b0e', side: THREE.DoubleSide });
    const wallpaperMap = wallpaper();
    const screenMaterial = new THREE.MeshBasicMaterial({ map: wallpaperMap, side: THREE.DoubleSide });

    const shellGeometry = new THREE.ExtrudeGeometry(roundedRect(2.04, 4.19, .27), {
        depth: .21, steps: 1, curveSegments: 18,
        bevelEnabled: true, bevelThickness: .035, bevelSize: .025, bevelSegments: 3
    });
    shellGeometry.center();
    group.add(new THREE.Mesh(shellGeometry, body));

    const frontBezel = new THREE.Mesh(new THREE.ShapeGeometry(roundedRect(1.94, 4.08, .24), 18), glass);
    frontBezel.position.z = .148;
    group.add(frontBezel);
    const displayGeometry = new THREE.ShapeGeometry(roundedRect(1.84, 3.98, .20), 18);
    const displayPositions = displayGeometry.getAttribute('position');
    const displayUvs = displayGeometry.getAttribute('uv');
    for (let i = 0; i < displayPositions.count; i++) {
        displayUvs.setXY(i, (displayPositions.getX(i) + .92) / 1.84, (displayPositions.getY(i) + 1.99) / 3.98);
    }
    displayUvs.needsUpdate = true;
    const display = new THREE.Mesh(displayGeometry, screenMaterial);
    display.position.z = .151;
    group.add(display);
    const island = new THREE.Mesh(new THREE.ShapeGeometry(roundedRect(.43, .084, .041), 12), glass);
    island.position.set(0, 1.86, .158);
    group.add(island);
    const sensor = new THREE.Mesh(new THREE.CircleGeometry(.018, 18), lens);
    sensor.position.set(.16, 1.86, .16);
    group.add(sensor);

    const rearPanel = new THREE.Mesh(new THREE.ShapeGeometry(roundedRect(1.91, 4.06, .23), 18), backGlass);
    rearPanel.rotation.y = Math.PI;
    rearPanel.position.z = -.15;
    group.add(rearPanel);

    // The camera plateau spans the width of the upper back, as on Apple's published design.
    const plateauGeometry = new THREE.ExtrudeGeometry(roundedRect(1.91, 1.05, .19), {
        depth: .055, curveSegments: 16, bevelEnabled: true,
        bevelSize: .018, bevelThickness: .012, bevelSegments: 2
    });
    plateauGeometry.center();
    const plateau = new THREE.Mesh(plateauGeometry, cameraPlate);
    plateau.position.set(0, 1.45, -.23);
    group.add(plateau);

    for (const [x, y] of [[.59, 1.72], [.13, 1.42], [.59, 1.14]]) {
        const outer = new THREE.Mesh(new THREE.CylinderGeometry(.197, .197, .072, 48), rim);
        outer.rotation.x = Math.PI / 2;
        outer.position.set(x, y, -.31);
        group.add(outer);
        const surround = new THREE.Mesh(new THREE.CylinderGeometry(.158, .158, .075, 48), dark);
        surround.rotation.x = Math.PI / 2;
        surround.position.set(x, y, -.36);
        group.add(surround);
        const optic = new THREE.Mesh(new THREE.CylinderGeometry(.119, .119, .078, 48), lens);
        optic.rotation.x = Math.PI / 2;
        optic.position.set(x, y, -.405);
        group.add(optic);
        const pupil = new THREE.Mesh(new THREE.CircleGeometry(.055, 36), glass);
        pupil.rotation.y = Math.PI;
        pupil.position.set(x, y, -.449);
        group.add(pupil);
        const glint = new THREE.Mesh(new THREE.CircleGeometry(.018, 20), new THREE.MeshBasicMaterial({ color: '#62809b', transparent: true, opacity: .75 }));
        glint.rotation.y = Math.PI;
        glint.position.set(x - .038, y + .038, -.452);
        group.add(glint);
    }
    const flashMaterial = new THREE.MeshBasicMaterial({ color: '#f9d9cc' });
    const flash = new THREE.Mesh(new THREE.CircleGeometry(.075, 28), flashMaterial);
    flash.rotation.y = Math.PI;
    flash.position.set(-.62, 1.7, -.283);
    group.add(flash);
    const lidar = new THREE.Mesh(new THREE.CircleGeometry(.073, 28), dark);
    lidar.rotation.y = Math.PI;
    lidar.position.set(-.62, 1.19, -.283);
    group.add(lidar);
    const mic = new THREE.Mesh(new THREE.CircleGeometry(.018, 20), dark);
    mic.rotation.y = Math.PI;
    mic.position.set(-.64, 1.44, -.283);
    group.add(mic);

    for (const [x, y, height] of [[-1.065, 1.1, .15], [-1.065, .53, .35], [-1.065, .05, .35], [1.065, .46, .52]]) {
        const button = new THREE.Mesh(new THREE.BoxGeometry(.045, height, .085), body);
        button.position.set(x, y, 0);
        group.add(button);
    }

    return {
        group,
        dispose() {
            const materials = new Set();
            group.traverse(object => {
                object.geometry?.dispose();
                if (object.material) materials.add(object.material);
            });
            materials.forEach(material => material.dispose());
            wallpaperMap.dispose();
        }
    };
}
