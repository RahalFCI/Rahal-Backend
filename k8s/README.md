# Rahal AKS Kubernetes Manifests

These manifests deploy the Rahal backend stack into the dedicated `rahal` namespace:

- .NET API as a stateless `Deployment`
- PostgreSQL as a persistent `StatefulSet`
- Redis as an ephemeral cache `Deployment`
- RabbitMQ as a persistent `StatefulSet`
- Meilisearch as a persistent `StatefulSet`
- NGINX Ingress + cert-manager TLS annotations

## Before Applying

1. Build and push the API image to your registry, then update `k8s/api/deployment.yaml`:

   ```bash
   image: <your-acr-name>.azurecr.io/rahal-api:<tag>
   ```

2. Copy the secret template to a private file and fill real values:

   ```bash
   cp k8s/secrets-template.yaml k8s/secrets.yaml
   ```

   Do not commit `k8s/secrets.yaml`.

3. Update the hostname in `k8s/ingress.yaml`:

   ```yaml
   host: api.rahal.example.com
   ```

4. Make sure `ingress-nginx` and `cert-manager` are already installed in the cluster.

   Install NGINX Ingress Controller:

   ```bash
   helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
   helm repo update
   helm install ingress-nginx ingress-nginx/ingress-nginx --namespace ingress-nginx --create-namespace -f k8s/ingress-nginx-values.yaml
   ```

   Install cert-manager:

   ```bash
   kubectl apply -f https://github.com/cert-manager/cert-manager/releases/latest/download/cert-manager.yaml
   ```

5. Update the email in `k8s/cluster-issuer.yaml` before applying it.

## Apply Order

Apply shared resources first:

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
```

Apply persistent dependencies:

```bash
kubectl apply -f k8s/postgres/pvc.yaml
kubectl apply -f k8s/postgres/service.yaml
kubectl apply -f k8s/postgres/statefulset.yaml

kubectl apply -f k8s/rabbitmq/pvc.yaml
kubectl apply -f k8s/rabbitmq/service.yaml
kubectl apply -f k8s/rabbitmq/statefulset.yaml

kubectl apply -f k8s/meilisearch/pvc.yaml
kubectl apply -f k8s/meilisearch/service.yaml
kubectl apply -f k8s/meilisearch/statefulset.yaml
```

Apply ephemeral cache:

```bash
kubectl apply -f k8s/redis/deployment.yaml
kubectl apply -f k8s/redis/service.yaml
```

Apply the API and autoscaling:

```bash
kubectl apply -f k8s/api/deployment.yaml
kubectl apply -f k8s/api/service.yaml
kubectl apply -f k8s/api/hpa.yaml
```

Apply the Let's Encrypt issuer and ingress last:

```bash
kubectl apply -f k8s/cluster-issuer.yaml
kubectl apply -f k8s/ingress.yaml
```

## Useful Checks

```bash
kubectl get all -n rahal
kubectl get pvc -n rahal
kubectl get ingress -n rahal
kubectl logs -n rahal deploy/rahal-api
```
