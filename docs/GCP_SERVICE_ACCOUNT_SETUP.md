# GCP Service Account Setup for CI/CD

## Required Permissions

Your GCP service account needs the following IAM roles:

### For Google Artifact Registry:
- `roles/artifactregistry.admin` - To create repositories and push/pull images
- `roles/artifactregistry.writer` - To push Docker images

### For Google Kubernetes Engine (GKE):
- `roles/container.admin` - To create and manage GKE clusters
- `roles/container.developer` - To deploy applications to GKE

### For Compute Engine (underlying GKE infrastructure):
- `roles/compute.admin` - To create compute resources for GKE nodes

## Steps to Create Service Account

1. **Create Service Account:**
   ```bash
   gcloud iam service-accounts create github-actions-sa \
     --description="Service account for GitHub Actions CI/CD" \
     --display-name="GitHub Actions SA"
   ```

2. **Assign Required Roles:**
   ```bash
   # Artifact Registry permissions
   gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
     --member="serviceAccount:github-actions-sa@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
     --role="roles/artifactregistry.admin"

   # GKE permissions
   gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
     --member="serviceAccount:github-actions-sa@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
     --role="roles/container.admin"

   # Compute permissions (for GKE nodes)
   gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
     --member="serviceAccount:github-actions-sa@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
     --role="roles/compute.admin"
   ```

3. **Create and Download Service Account Key:**
   ```bash
   gcloud iam service-accounts keys create github-actions-key.json \
     --iam-account=github-actions-sa@YOUR_PROJECT_ID.iam.gserviceaccount.com
   ```

4. **GitHub Secrets Setup:**
   - `GCP_SA_KEY`: Copy the entire content of `github-actions-key.json` (including {})
   - `GCP_PROJECT_ID`: Your GCP project ID
   - `GKE_ZONE`: The zone where you want to create your GKE cluster (e.g., `us-central1-a`)
   - `REGISTRY_REGION`: The region for Artifact Registry (e.g., `us-central1` - without zone suffix)

## Security Notes

- Never commit the service account key file to your repository
- Use GitHub secrets to store sensitive information
- Consider using Workload Identity for enhanced security in production
- Regularly rotate service account keys

## Artifact Registry vs Container Registry

This setup uses Google Artifact Registry (recommended) instead of the legacy Container Registry (gcr.io). Artifact Registry provides:
- Better security and access controls
- Support for multiple artifact types
- Regional and multi-regional repositories
- Integration with Cloud Build and other GCP services

## 🗺️ **GCP Zones vs Regions:**
- **GKE uses zones** (with `-a`, `-b` suffix): `us-central1-a`, `europe-west1-a`
- **Artifact Registry uses regions** (without zone suffix): `us-central1`, `europe-west1`

**Common Examples:**
- US Central: Zone `us-central1-a` → Region `us-central1`
- Europe West: Zone `europe-west1-a` → Region `europe-west1`
- Asia Southeast: Zone `asia-southeast1-a` → Region `asia-southeast1`
